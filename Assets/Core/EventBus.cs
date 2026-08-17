using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keres.Core
{
    /// <summary>
    /// Event Bus Pattern
    ///
    /// Mục đích:
    /// - Cho các hệ thống trong game giao tiếp với nhau.
    /// - Không cần các class phải biết trực tiếp về nhau.
    ///
    /// Ví dụ:
    ///
    /// Player
    ///    │
    ///    │ Emit(PlayerDamagedEvent)
    ///    ↓
    /// EventBus
    ///    │
    ///    ├──→ UIManager
    ///    ├──→ AudioManager
    ///    └──→ CameraShake
    ///
    /// Player không cần biết UIManager, AudioManager
    /// hay CameraShake tồn tại.
    /// </summary>
    public static class EventBus
    {
        // ============================================================
        // 1. EVENT STORAGE
        // ============================================================

        /*
         * Dictionary<Type, Delegate>
         *
         * Key:
         *      Type của Event
         *
         * Value:
         *      Delegate chứa các Handler đã Subscribe
         *
         * Ví dụ:
         *
         * PlayerDamagedEvent
         *          ↓
         *      UpdateHealthUI()
         *      PlayDamageSound()
         *      ShakeCamera()
         *
         *
         * Tại sao dùng Type?
         *
         * Vì mỗi loại Event có một "kênh" riêng.
         *
         * PlayerDamagedEvent → kênh A
         * EnemyDiedEvent     → kênh B
         * GameOverEvent      → kênh C
         */

        private static readonly Dictionary<Type, Delegate> channels = new();


        // ============================================================
        // 2. SUBSCRIBE
        // ============================================================

        /// <summary>
        /// Đăng ký một Handler để lắng nghe Event T.
        ///
        /// Khi Event T được Emit,
        /// Handler này sẽ được gọi.
        /// </summary>
        ///
        /// <typeparam name="T">
        /// Kiểu Event.
        ///
        /// T phải là struct.
        /// </typeparam>
        ///
        /// <param name="handler">
        /// Hàm sẽ được gọi khi Event xảy ra.
        /// </param>
        public static void Subscribe<T>(Action<T> handler)
            where T : struct
        {
            // Tìm danh sách Handler hiện tại của Event T.
            channels.TryGetValue(typeof(T), out var existingDelegate);

            // Thêm Handler mới vào danh sách.
            channels[typeof(T)] =
                Delegate.Combine(existingDelegate, handler);
        }


        // ============================================================
        // 3. UNSUBSCRIBE
        // ============================================================

        /// <summary>
        /// Hủy đăng ký một Handler khỏi Event T.
        ///
        /// Thường gọi trong OnDisable / OnDestroy
        /// để tránh Handler bị gọi khi Object không còn tồn tại.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
            where T : struct
        {
            // Tìm danh sách Handler của Event T.
            if (channels.TryGetValue(typeof(T), out var existingDelegate))
            {
                // Xóa Handler khỏi danh sách.
                channels[typeof(T)] =
                    Delegate.Remove(existingDelegate, handler);
            }
        }


        // ============================================================
        // 4. EMIT
        // ============================================================

        /// <summary>
        /// Phát một Event.
        ///
        /// Tất cả Handler đang Subscribe Event T
        /// sẽ được gọi.
        /// </summary>
        ///
        /// <param name="evt">
        /// Dữ liệu của Event.
        /// </param>
        public static void Emit<T>(T evt)
            where T : struct
        {
            // Tìm danh sách Handler của Event T.
            if (channels.TryGetValue(typeof(T), out var existingDelegate))
            {
                // Delegate đang được lưu dưới kiểu Delegate.
                // Ép nó trở lại thành Action<T>
                // để có thể truyền Event vào Handler.
                ((Action<T>)existingDelegate)?.Invoke(evt);
            }
        }
    }
}