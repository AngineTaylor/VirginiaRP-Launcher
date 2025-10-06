using Launcher.ServiceLib.Data;
using System;

namespace Admin.ViewModelsAdmin
{
    public class AdminCharacterViewModel
    {
        private readonly CharacterData _character;

        public AdminCharacterViewModel(CharacterData character)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));

            Id = character.Id;
            ShortId = character.ShortId;
            Nickname = character.Nickname;
            Age = character.Age;
            Story = character.Story;
            SteamId64 = character.SteamId64;
            RegIp = character.RegIp;
            CreatedAt = character.CreatedAt;

            // Если есть логика определения онлайн-статуса
            IsOnline = false; // Пока заглушка
        }

        public int Id { get; }
        public int ShortId { get; }
        public string Nickname { get; }
        public int Age { get; }
        public string Story { get; }
        public long SteamId64 { get; }
        public string RegIp { get; }
        public DateTime CreatedAt { get; }
        public bool IsOnline { get; set; }
    }
}
