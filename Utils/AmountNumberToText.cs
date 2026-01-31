namespace NavetraERP.Utils;

/* public static class NumberToHungarianText
{
    private static readonly string[] Ones =
    {
        "nulla", "egy", "kettő", "három", "négy",
        "öt", "hat", "hét", "nyolc", "kilenc"
    };

    private static readonly string[] Tens =
    {
        "", "tíz", "húsz", "harminc", "negyven",
        "ötven", "hatvan", "hetven", "nyolcvan", "kilencven"
    };

    public static string ToText(int number)
    {
        if (number == 0)
            return Ones[0];

        if (number < 0)
            return "mínusz " + ToText(Math.Abs(number));

        return Convert(number).Trim();
    }

    private static string Convert(int number)
    {
        if (number < 10)
            return Ones[number];

        if (number < 20)
        {
            if (number == 10) return "tíz";
            if (number == 20) return "húsz";
            return "tizen" + Ones[number - 10];
        }

        if (number < 100)
        {
            int ten = number / 10;
            int rest = number % 10;
            return Tens[ten] + (rest > 0 ? Ones[rest] : "");
        }

        if (number < 1000)
        {
            int hundred = number / 100;
            int rest = number % 100;

            string result = hundred == 1 ? "száz" : Ones[hundred] + "száz";
            return result + (rest > 0 ? Convert(rest) : "");
        }

        if (number < 1_000_000)
        {
            int thousand = number / 1000;
            int rest = number % 1000;

            string result = thousand == 1 ? "ezer" : Convert(thousand) + "ezer";
            return result + (rest > 0 ? Convert(rest) : "");
        }

        if (number < 1_000_000_000)
        {
            int million = number / 1_000_000;
            int rest = number % 1_000_000;

            string result = Convert(million) + "millió";
            return result + (rest > 0 ? Convert(rest) : "");
        }

        return number.ToString(); // túl nagy szám esetén
    }
} */

public static class NumberToHungarianText
{
    private static readonly string[] Ones =
    {
        "nulla", "egy", "kettő", "három", "négy",
        "öt", "hat", "hét", "nyolc", "kilenc"
    };

    private static readonly string[] Tens =
    {
        "", "tíz", "húsz", "harminc", "negyven",
        "ötven", "hatvan", "hetven", "nyolcvan", "kilencven"
    };

    public static string ToText(int number)
    {
        if (number == 0)
            return Ones[0];

        if (number < 0)
            return "mínusz-" + ToText(Math.Abs(number));

        return Convert(number);
    }

    private static string Convert(int number)
    {
        if (number < 10)
            return Ones[number];

        if (number < 20)
            return number == 10 ? "tíz" : "tizen" + Ones[number - 10];

        if (number < 100)
        {
            int ten = number / 10;
            int rest = number % 10;
            return Tens[ten] + (rest > 0 ? Ones[rest] : "");
        }

        if (number < 1000)
        {
            int hundred = number / 100;
            int rest = number % 100;

            string result = hundred == 1 ? "száz" : Ones[hundred] + "száz";
            return rest > 0 ? result + Convert(rest) : result;
        }

        if (number < 1_000_000)
        {
            int thousand = number / 1000;
            int rest = number % 1000;

            string result = thousand == 1 ? "ezer" : Convert(thousand) + "ezer";

            // 1000 felett kötőjel, ha van maradék
            return rest > 0 ? result + "-" + Convert(rest) : result;
        }

        int million = number / 1_000_000;
        int remainder = number % 1_000_000;

        string millionText = Convert(million) + "millió";
        return remainder > 0 ? millionText + "-" + Convert(remainder) : millionText;
    }
}

