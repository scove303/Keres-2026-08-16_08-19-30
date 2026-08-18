using UnityEngine;

namespace Assets.Tools.Folder
{
    /// <summary>
    /// Chứa toàn bộ hằng số dùng chung của hệ thống Folder Icon.
    /// Mục đích: gom hết "số ma thuật" (magic number) vào một chỗ
    /// để sau này muốn chỉnh size, offset, màu sắc... chỉ cần sửa ở đây.
    /// </summary>
    public static class FolderIconConstants
    {
        //  ========================================================
        // MENU (menu Create Asset trong Project Window)
        //  ========================================================
        /// <summary>
        /// Tên mặc định của file asset khi tạo qua menu Create
        /// </summary>
        public const string MenuFileName = "Folder Icon Manager";

        /// <summary>
        /// Vị trí menu khi chuột phải vào Project Window
        /// -> Create -> Tools -> Folder Icon Manager
        /// </summary>
        public const string MenuPath = "Tools/Folder Icon Manager";

        //  ========================================================
        // KÍCH THƯỚC ICON (đơn vị: pixel)
        //  ========================================================
        /// <summary>Kích thước icon ở chế độ danh sách (TreeView - xem dọc)</summary>
        public const float MaxTreeWidth = 118f;
        public const float MaxTreeHeight = 16f;

        /// <summary>Kích thước icon ở chế độ lưới (GridView - xem ngang)</summary>
        public const float MaxProjectWidth = 96f;
        public const float MaxProjectHeight = 110f;

        //  ========================================================
        // OFFSET (vị trí dịch chuyển thêm của icon)
        //  ========================================================
        /// <summary>
        /// Đẩy icon sang phải 3px để không đè lên tên thư mục
        /// (chỉ áp dụng ở chế độ TreeView)
        /// </summary>
        public const float TreeViewXOffset = 3f;

        //  ========================================================
        // MÀU SẮC
        //  ========================================================
        /// <summary>
        /// Màu nền xanh khi một mục icon đang được chọn trong Inspector
        /// </summary>
        public static readonly Color SelectedColor = new Color(0.235f, 0.360f, 0.580f);
    }
}
