# 🎵 Magic Tiles 3 Test (Unity)

## 📌 Giới thiệu
Dự án này là một game rhythm dạng Magic Tiles 3, nơi người chơi nhấn vào các nốt rơi theo nhịp nhạc:
- Hệ thống Beat Map đọc từ file JSON.(JSON được tạo từ file beat(labels) export từ Audacity)
- UI quản lý trạng thái game (Menu, Playing, Pause, Result, Lose, Win).
- Hệ thống điểm + streak combo + high score.
- Countdown trước khi bắt đầu game.
- Hệ thống lưu dữ liệu (high score, nhạc đã chọn).
- Quản lý danh sách nhạc để người chơi lựa chọn.
- Quản lý pool tile để tối ưu hiệu năng.

⚙ Quyết định thiết kế:
- Beat Map JSON: Dễ dàng tạo mới và chỉnh sửa, tách biệt logic game với dữ liệu nhạc.
- Object Pooling: Giảm load CPU khi spawn tile.
- Game State Machine: Tách biệt logic các trạng thái (Menu, Playing, Pause, Win, Lose) → dễ mở rộng.
- Countdown trước game: Giúp người chơi chuẩn bị.
- Save System: Lưu high score và bài nhạc đã chọn.(lưu dưới dạng JSON(local))
- Music Manager: Quản lý danh sách bài hát và phát nhạc theo lựa chọn của người chơi.
- Sử dụng các pattern: Singleton Pattern, State Pattern, Object Pool Pattern, Observer/Event Pattern, Factory-like Spawner.
- Các nguyên tác thiết kế (OOP + SOLID).
---
Nguồn gốc asset & script:
- UI & Sound, Font: Tự tạo và lấy từ nguồn internet.
- Music: Lấy từ Youtube
- DOTween: DOTween (Demigiant) - Free Asset Store.

---
Tool: Audacity, Photoshop, Unity(version 2021.3.3f1)...
Language: C#

Script & Logic: Tự viết, AI. Kiến trúc tổng thể xây dựng từ kinh nghiệm qua 1 vài dự án cá nhân.
