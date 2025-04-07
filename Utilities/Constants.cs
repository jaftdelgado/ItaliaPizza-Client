namespace ItaliaPizzaClient.Utilities
{
    public static class Constants
    {
        public static readonly string DEFAULT_PROFILE_PIC_PATH = "/Resources/Images/default-profile-pic.png";
        public static readonly string DEFAULT_SUPPLY_PIC_PATH = "/Resources/Images/default-supply-pic.png";

        public static readonly long MAX_IMAGE_SIZE = 5 * 1024 * 1024;
        public static readonly int MAX_LENGTH_RFC = 13;
        public static readonly int MAX_LENGTH_NAMES = 32;
        public static readonly int MAX_LENGTH_USERNAME = 16;

        public static readonly string IMAGE_FILE_FILTER = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

        #region STRING-PATTERNS
        public static readonly string ALPHANUMERIC_PATTERN = @"[^a-zA-Z0-9]";
        public static readonly string NAMES_PATTERN = @"[^A-Za-zÁÉÍÓÚÜÑáéíóúüñ' -]";
        public static readonly string SAFE_PASSWORD_PATTERN = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$";
        #endregion
    }
}