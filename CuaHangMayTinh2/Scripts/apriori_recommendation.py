import sys
import pandas as pd
import pyodbc
import json
import logging
import time
from mlxtend.frequent_patterns import apriori, association_rules
from mlxtend.preprocessing import TransactionEncoder
from datetime import datetime

# Thiết lập logging để ghi log vào stderr thay vì stdout
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    stream=sys.stderr  # Ghi log vào stderr
)

# Đo thời gian thực thi
start_time = time.time()
logging.info("Bắt đầu chạy thuật toán Apriori")

# Đọc min_support từ tham số dòng lệnh
if len(sys.argv) > 1:
    try:
        min_support = float(sys.argv[1])
        if min_support <= 0 or min_support > 1:
            raise ValueError("min_support phải nằm trong khoảng (0, 1]")
        logging.info(f"Sử dụng min_support = {min_support}")
    except ValueError as e:
        logging.error(f"Giá trị min_support không hợp lệ: {sys.argv[1]}. Lỗi: {str(e)}")
        print(json.dumps({"error": f"Giá trị min_support không hợp lệ: {sys.argv[1]}. Lỗi: {str(e)}"}))
        sys.exit(1)
else:
    min_support = 0.01  # Giá trị mặc định
    logging.info(f"Không có tham số min_support, sử dụng giá trị mặc định: {min_support}")

# Đọc min_confidence từ tham số dòng lệnh (nếu có)
if len(sys.argv) > 2:
    try:
        min_confidence = float(sys.argv[2])
        if min_confidence <= 0 or min_confidence > 1:
            raise ValueError("min_confidence phải nằm trong khoảng (0, 1]")
        logging.info(f"Sử dụng min_confidence = {min_confidence}")
    except ValueError as e:
        logging.error(f"Giá trị min_confidence không hợp lệ: {sys.argv[2]}. Lỗi: {str(e)}")
        print(json.dumps({"error": f"Giá trị min_confidence không hợp lệ: {sys.argv[2]}. Lỗi: {str(e)}"}))
        sys.exit(1)
else:
    min_confidence = 0.2  # Giá trị mặc định
    logging.info(f"Không có tham số min_confidence, sử dụng giá trị mặc định: {min_confidence}")

# Đọc min_lift từ tham số dòng lệnh (nếu có)
if len(sys.argv) > 3:
    try:
        min_lift = float(sys.argv[3])
        if min_lift < 1:
            raise ValueError("min_lift phải lớn hơn hoặc bằng 1")
        logging.info(f"Sử dụng min_lift = {min_lift}")
    except ValueError as e:
        logging.error(f"Giá trị min_lift không hợp lệ: {sys.argv[3]}. Lỗi: {str(e)}")
        print(json.dumps({"error": f"Giá trị min_lift không hợp lệ: {sys.argv[3]}. Lỗi: {str(e)}"}))
        sys.exit(1)
else:
    min_lift = 1.0  # Giá trị mặc định
    logging.info(f"Không có tham số min_lift, sử dụng giá trị mặc định: {min_lift}")

# Đọc category_id từ tham số dòng lệnh (nếu có)
category_id = None
if len(sys.argv) > 4 and sys.argv[4] != "0":
    try:
        category_id = int(sys.argv[4])
        logging.info(f"Lọc theo danh mục ID = {category_id}")
    except ValueError as e:
        logging.error(f"Giá trị category_id không hợp lệ: {sys.argv[4]}. Lỗi: {str(e)}")
        print(json.dumps({"error": f"Giá trị category_id không hợp lệ: {sys.argv[4]}. Lỗi: {str(e)}"}))
        sys.exit(1)

# Kết nối tới SQL Server
try:
    logging.info("Đang kết nối tới SQL Server...")
    # Thử nhiều driver ODBC khác nhau
    drivers = [
        '{ODBC Driver 17 for SQL Server}',
        '{ODBC Driver 13 for SQL Server}',
        '{SQL Server}',
        '{SQL Server Native Client 11.0}'
    ]
    
    conn = None
    connection_error = None
    
    for driver in drivers:
        try:
            logging.info(f"Thử kết nối với driver: {driver}")
            conn = pyodbc.connect(
                f'DRIVER={driver};'
                'SERVER=NATSUKI-ZUBARU\\SQLEXPRESS;'
                'DATABASE=CuaHangMayTinh2;'
                'Trusted_Connection=yes;'
            )
            logging.info(f"Kết nối thành công với driver: {driver}")
            break
        except Exception as e:
            connection_error = str(e)
            logging.warning(f"Không thể kết nối với driver {driver}: {str(e)}")
    
    if conn is None:
        raise Exception(f"Không thể kết nối với bất kỳ driver nào. Lỗi cuối cùng: {connection_error}")
    
    cursor = conn.cursor()
    logging.info("Kết nối SQL Server thành công!")
except Exception as e:
    logging.error(f"Không thể kết nối tới SQL Server: {str(e)}")
    print(json.dumps({"error": f"Không thể kết nối tới SQL Server: {str(e)}"}))
    sys.exit(1)

# Truy vấn dữ liệu từ bảng ChiTietDonHang và thông tin sản phẩm
try:
    logging.info("Đang truy vấn dữ liệu từ bảng ChiTietDonHang và SanPham...")
    
    # Xây dựng câu truy vấn dựa trên có lọc theo danh mục hay không
    if category_id is not None:
        query = """
        SELECT ctdh.DonHangID, ctdh.SanPhamID, sp.TenSP, sp.DanhMucID
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp ON ctdh.SanPhamID = sp.SanPhamID
        JOIN DonHang dh ON ctdh.DonHangID = dh.DonHangID
        WHERE dh.TrangThaiDonHang = 'Completed'
        """
        logging.info(f"Sử dụng query có lọc theo danh mục: {query}")
    else:
        query = """
        SELECT ctdh.DonHangID, ctdh.SanPhamID, sp.TenSP
        FROM ChiTietDonHang ctdh
        JOIN SanPham sp ON ctdh.SanPhamID = sp.SanPhamID
        JOIN DonHang dh ON ctdh.DonHangID = dh.DonHangID
        WHERE dh.TrangThaiDonHang = 'Completed'
        """
        logging.info(f"Sử dụng query không lọc theo danh mục: {query}")
    
    # Thực hiện truy vấn
    logging.info("Bắt đầu thực hiện truy vấn...")
    df = pd.read_sql(query, conn)
    logging.info(f"Truy vấn thành công. Số lượng bản ghi: {len(df)}")
    
    # Kiểm tra dữ liệu truy vấn
    if len(df) == 0:
        logging.error("Không có dữ liệu đơn hàng nào được trả về từ truy vấn!")
        print(json.dumps({"error": "Không có dữ liệu đơn hàng nào được trả về từ truy vấn!"}))
        conn.close()
        sys.exit(1)
    
    # Hiển thị một số bản ghi đầu tiên để debug
    logging.info(f"Mẫu dữ liệu: {df.head().to_dict()}")
    
    # Lọc theo danh mục nếu có yêu cầu
    if category_id is not None:
        # Lấy danh sách các sản phẩm thuộc danh mục đã chọn
        category_products = df[df['DanhMucID'] == category_id]['SanPhamID'].unique()
        logging.info(f"Số lượng sản phẩm thuộc danh mục {category_id}: {len(category_products)}")
        
        if len(category_products) == 0:
            logging.error(f"Không có sản phẩm nào thuộc danh mục {category_id}")
            print(json.dumps({"error": f"Không có sản phẩm nào thuộc danh mục {category_id}"}))
            conn.close()
            sys.exit(1)
        
        # Lọc các giao dịch có chứa ít nhất một sản phẩm thuộc danh mục đã chọn
        relevant_transactions = df[df['SanPhamID'].isin(category_products)]['DonHangID'].unique()
        logging.info(f"Số lượng giao dịch liên quan: {len(relevant_transactions)}")
        
        # Lọc dữ liệu chỉ giữ lại các giao dịch liên quan
        df = df[df['DonHangID'].isin(relevant_transactions)]
        logging.info(f"Số lượng bản ghi sau khi lọc: {len(df)}")
    
    # Truy vấn thông tin hình ảnh sản phẩm
    image_query = """
    SELECT ha.SanPhamID, ha.DuongDanAnh
    FROM HinhAnhSanPham ha
    INNER JOIN (
        SELECT SanPhamID, MIN(HinhAnhID) as MinHinhAnhID
        FROM HinhAnhSanPham
        WHERE LaHinhChinh = 1
        GROUP BY SanPhamID
    ) t ON ha.SanPhamID = t.SanPhamID AND ha.HinhAnhID = t.MinHinhAnhID
    """
    logging.info(f"Truy vấn hình ảnh: {image_query}")
    image_df = pd.read_sql(image_query, conn)
    logging.info(f"Số lượng hình ảnh: {len(image_df)}")
    
    # Tạo dictionary ánh xạ SanPhamID -> DuongDanAnh
    image_dict = dict(zip(image_df['SanPhamID'], image_df['DuongDanAnh']))
    logging.info(f"Đã tạo dictionary ánh xạ SanPhamID -> DuongDanAnh với {len(image_dict)} mục")
    
except Exception as e:
    logging.error(f"Lỗi khi truy vấn dữ liệu: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi truy vấn dữ liệu: {str(e)}"}))
    conn.close()
    sys.exit(1)

# Đóng kết nối
conn.close()

# Kiểm tra dữ liệu đầu vào
if df.empty:
    error_msg = "Không có dữ liệu đơn hàng!"
    logging.error(error_msg)
    print(json.dumps({"error": error_msg}))
    sys.exit(1)

# Tạo danh sách giao dịch
try:
    logging.info("Đang tạo danh sách giao dịch...")
    transactions = df.groupby('DonHangID')['SanPhamID'].apply(list).tolist()
    logging.info(f"Số lượng giao dịch: {len(transactions)}")
    
    # Tạo dictionary ánh xạ SanPhamID -> TenSP
    product_names = dict(zip(df['SanPhamID'], df['TenSP']))
    
    if not transactions:
        error_msg = "Không có giao dịch nào được tạo từ dữ liệu!"
        logging.error(error_msg)
        print(json.dumps({"error": error_msg}))
        sys.exit(1)
except Exception as e:
    logging.error(f"Lỗi khi tạo danh sách giao dịch: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi tạo danh sách giao dịch: {str(e)}"}))
    sys.exit(1)

# Chuyển đổi dữ liệu thành one-hot encoding sử dụng TransactionEncoder
try:
    logging.info("Đang chuyển đổi dữ liệu thành one-hot encoding...")
    te = TransactionEncoder()
    te_ary = te.fit(transactions).transform(transactions)
    df_onehot = pd.DataFrame(te_ary, columns=te.columns_)
    logging.info("Chuyển đổi one-hot encoding thành công!")
except Exception as e:
    logging.error(f"Lỗi khi chuyển đổi dữ liệu thành one-hot encoding: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi chuyển đổi dữ liệu thành one-hot encoding: {str(e)}"}))
    sys.exit(1)

# Chạy thuật toán Apriori
try:
    logging.info(f"Đang chạy thuật toán Apriori với min_support={min_support}...")
    frequent_itemsets = apriori(df_onehot, min_support=min_support, use_colnames=True)
    logging.info(f"Số lượng tập phổ biến (frequent itemsets): {len(frequent_itemsets)}")
    if frequent_itemsets.empty:
        error_msg = f"Không tìm thấy tập phổ biến nào với min_support={min_support}!"
        logging.error(error_msg)
        print(json.dumps({"error": error_msg}))
        sys.exit(1)
except Exception as e:
    logging.error(f"Lỗi khi chạy thuật toán Apriori: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi chạy thuật toán Apriori: {str(e)}"}))
    sys.exit(1)

# Tạo luật kết hợp
try:
    logging.info(f"Đang tạo luật kết hợp với min_confidence={min_confidence} và min_lift={min_lift}...")
    rules = association_rules(frequent_itemsets, metric="confidence", min_threshold=min_confidence)
    
    # Lọc theo min_lift
    rules = rules[rules['lift'] >= min_lift]
    
    logging.info(f"Số lượng luật kết hợp sau khi lọc: {len(rules)}")
    if rules.empty:
        error_msg = f"Không tìm thấy luật kết hợp nào với min_confidence={min_confidence} và min_lift={min_lift}!"
        logging.error(error_msg)
        print(json.dumps({"error": error_msg}))
        sys.exit(1)
except Exception as e:
    logging.error(f"Lỗi khi tạo luật kết hợp: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi tạo luật kết hợp: {str(e)}"}))
    sys.exit(1)

# Chuẩn hóa kết quả và loại bỏ trùng lặp
try:
    logging.info("Đang chuẩn hóa kết quả...")
    results_dict = {}  # Sử dụng dictionary để loại bỏ trùng lặp và chọn luật tốt nhất
    transaction_counts = {}  # Đếm số lần xuất hiện của cặp sản phẩm
    
    # Đếm số lần mua cùng của từng cặp sản phẩm
    for transaction in transactions:
        for i, item1 in enumerate(transaction):
            for item2 in transaction[i+1:]:
                if item1 != item2:
                    pair = tuple(sorted([item1, item2]))  # Sắp xếp để tránh trùng lặp (1,2) và (2,1)
                    transaction_counts[pair] = transaction_counts.get(pair, 0) + 1

    # Chuẩn hóa luật kết hợp
    for _, row in rules.iterrows():
        antecedents = list(row['antecedents'])
        consequents = list(row['consequents'])
        for antecedent in antecedents:
            for consequent in consequents:
                if antecedent != consequent:
                    pair = tuple(sorted([antecedent, consequent]))
                    key = f"{antecedent}-{consequent}"
                    if key not in results_dict or row['lift'] > results_dict[key]['Lift']:
                        so_lan_mua_cung = transaction_counts.get(pair, 0)
                        
                        # Lấy tên sản phẩm
                        ten_sp1 = product_names.get(antecedent, f"Sản phẩm {antecedent}")
                        ten_sp2 = product_names.get(consequent, f"Sản phẩm {consequent}")
                        
                        # Lấy đường dẫn hình ảnh
                        hinh_anh1 = image_dict.get(antecedent, "/images/products/default.jpg")
                        hinh_anh2 = image_dict.get(consequent, "/images/products/default.jpg")
                        
                        results_dict[key] = {
                            "SanPhamID1": int(antecedent),
                            "SanPhamID2": int(consequent),
                            "TenSP1": ten_sp1,
                            "TenSP2": ten_sp2,
                            "SoLanMuaCung": so_lan_mua_cung,
                            "Support": float(row['support']),
                            "Confidence": float(row['confidence']),
                            "Lift": float(row['lift']),
                            "HinhAnh1": hinh_anh1,
                            "HinhAnh2": hinh_anh2
                        }

    results = list(results_dict.values())
    # Sắp xếp kết quả theo Lift giảm dần
    results.sort(key=lambda x: x['Lift'], reverse=True)
    logging.info(f"Tìm thấy {len(results)} luật kết hợp sau khi chuẩn hóa.")
except Exception as e:
    logging.error(f"Lỗi khi chuẩn hóa kết quả: {str(e)}")
    print(json.dumps({"error": f"Lỗi khi chuẩn hóa kết quả: {str(e)}"}))
    sys.exit(1)

# Ghi log thời gian thực thi
elapsed_time = time.time() - start_time
logging.info(f"Hoàn thành thuật toán Apriori trong {elapsed_time:.2f} giây")

# In kết quả dưới dạng JSON vào stdout
print(json.dumps(results, ensure_ascii=False))
