namespace xObsAsyncImageSource.SocketUtils.Helper
{
    public class CharacterInfo
    {
        public string AvatarName { get; set; }
        public string AvatarFeature { get; set; }
        public int AvatarIndex { get; set; }
        public int AvatarAge { get; set; }

        public CharacterInfo(string name, string feature, int index, int age = 18)
        {
            AvatarName = name;
            AvatarFeature = feature;
            AvatarIndex = index;
            AvatarAge = age;
        }
    }
}
