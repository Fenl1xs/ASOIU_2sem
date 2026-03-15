using System;

class Program
{
    static int Distance(string str1Param, string str2Param)
    {
        if ((str1Param == null) || (str2Param == null))
            return -1;

        int str1Len = str1Param.Length;
        int str2Len = str2Param.Length;

        if ((str1Len == 0) && (str2Len == 0))
            return 0;
        if (str1Len == 0)
            return str2Len;
        if (str2Len == 0)
            return str1Len;

        string str1 = str1Param.ToUpper();
        string str2 = str2Param.ToUpper();

        int[,] matrix = new int[str1Len + 1, str2Len + 1];

        for (int i = 0; i <= str1Len; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= str2Len; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= str1Len; i++)
        {
            for (int j = 1; j <= str2Len; j++)
            {
                int symbEqual = (str1.Substring(i - 1, 1) == str2.Substring(j - 1, 1)) ? 0 : 1;

                int ins = matrix[i, j - 1] + 1;
                int del = matrix[i - 1, j] + 1;
                int subst = matrix[i - 1, j - 1] + symbEqual;

                matrix[i, j] = Math.Min(Math.Min(ins, del), subst);

                if ((i > 1) && (j > 1) &&
                    (str1.Substring(i - 1, 1) == str2.Substring(j - 2, 1)) &&
                    (str1.Substring(i - 2, 1) == str2.Substring(j - 1, 1)))
                {
                    matrix[i, j] = Math.Min(matrix[i, j],
                        matrix[i - 2, j - 2] + symbEqual);
                }
            }
        }
        return matrix[str1Len, str2Len];
    }

    static void WriteDistance(string str1Param, string str2Param)
    {
        int d = Distance(str1Param, str2Param);
        Console.WriteLine("'" + str1Param + "', '" + str2Param + "' -> " + d.ToString());
    }

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nПоиск с опечатками (расстояние Дамерау-Левенштейна) ");
            Console.Write("Введите первую строку (или 'exit' для выхода): ");
            string? str1 = Console.ReadLine();

            if (str1?.ToLower() == "exit")
            {
                Console.WriteLine("Программа завершена.");
                break;
            }

            Console.Write("Введите вторую строку: ");
            string? str2 = Console.ReadLine();

            if (str1 == null || str2 == null)
            {
                Console.WriteLine("Ошибка ввода.");
                continue;
            }

            WriteDistance(str1, str2);
        }
    }
}