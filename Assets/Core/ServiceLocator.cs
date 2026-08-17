using System;
using System.Collections.Generic;

namespace Keres.Core
{
    /// <summary>
    /// Service Locator Pattern
    ///
    /// Mục đích:
    /// - Lưu trữ các Service/Manager dùng chung.
    /// - Cho phép lấy Service ở bất cứ đâu.
    /// - Không cần tạo Singleton cho từng Manager.
    /// - Không cần Drag & Drop Manager vào từng Script.
    ///
    /// Cách hoạt động:
    ///     Register<T>()   → Đăng ký Service
    ///     Get<T>()        → Lấy Service
    ///     Unregister<T>() → Xóa Service
    /// </summary>
    public static class ServiceLocator
    {
        // ============================================================
        // 1. STORAGE
        // ============================================================

        /*
         * Dictionary:
         *
         * Key   = Type
         * Value = object
         *
         * Ví dụ:
         *
         * typeof(AudioManager)
         *          ↓
         *      AudioManager instance
         *
         * typeof(SaveSystem)
         *          ↓
         *       SaveSystem instance
         */

        private static readonly Dictionary<Type, object> services = new();


        // ============================================================
        // 2. REGISTER
        // ============================================================

        /// <summary>
        /// Đăng ký một Service vào Service Locator.
        /// </summary>
        /// 
        /// <typeparam name="T">
        /// Kiểu của Service cần đăng ký.
        /// </typeparam>
        ///
        /// <param name="instance">
        /// Instance thực tế của Service.
        /// </param>
        public static void Register<T>(T instance) where T : class
        {
            services[typeof(T)] = instance;
        }


        // ============================================================
        // 3. UNREGISTER
        // ============================================================

        /// <summary>
        /// Xóa một Service khỏi Service Locator.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            services.Remove(typeof(T));
        }


        // ============================================================
        // 4. GET
        // ============================================================

        /// <summary>
        /// Lấy Service đã đăng ký.
        ///
        /// Nếu Service chưa được đăng ký → throw Exception.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            throw new InvalidOperationException(
                $"Service of type {typeof(T)} is not registered."
            );
        }
    }
}