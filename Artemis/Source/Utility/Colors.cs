public static class Colors
{
    public enum ColorEnum
    {
        Pink = 0xE91E63,
        Blue = 0x3498DB,
        Blurple = 0x5865F2,
        Green = 0x57F287,
        Red = 0xED4245,
        Yellow = 0xFEE75C,
        Orange = 0xF47C3C,
        Fuchsia = 0xEB459E,
        White = 0xFFFFFF,
        Black = 0x000000,
        Gray = 0x747F8D,
        DarkGray = 0x2F3136,
        LightGray = 0x99AAB5,
        DarkerGray = 0x23272A,
        NotQuiteWhite = 0x36393F
    }

    public static NetCord.Color Pink => new((int)ColorEnum.Pink);
    public static NetCord.Color Blue => new((int)ColorEnum.Blue);
    public static NetCord.Color Blurple => new((int)ColorEnum.Blurple);
    public static NetCord.Color Green => new((int)ColorEnum.Green);
    public static NetCord.Color Red => new((int)ColorEnum.Red);
    public static NetCord.Color Yellow => new((int)ColorEnum.Yellow);
    public static NetCord.Color Orange => new((int)ColorEnum.Orange);
    public static NetCord.Color Fuchsia => new((int)ColorEnum.Fuchsia);
    public static NetCord.Color White => new((int)ColorEnum.White);
    public static NetCord.Color Black => new((int)ColorEnum.Black);
    public static NetCord.Color Gray => new((int)ColorEnum.Gray);
    public static NetCord.Color DarkGray => new((int)ColorEnum.DarkGray);
    public static NetCord.Color LightGray => new((int)ColorEnum.LightGray);
    public static NetCord.Color DarkerGray => new((int)ColorEnum.DarkerGray);
    public static NetCord.Color NotQuiteWhite => new((int)ColorEnum.NotQuiteWhite);

}
