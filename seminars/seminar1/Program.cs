using System;

string[] teams = new string[0];
double[] speeds = new double[0];

Console.WriteLine("=== анализ квалификации гран-при ===");
Console.WriteLine();

Console.Write("введите количество участников: ");
int n = int.Parse(Console.ReadLine() ?? "0");
Console.WriteLine();

teams = new string[n];
speeds = new double[n];

for (int i = 0; i < n; i++)
{
    Console.WriteLine($"участник #{i + 1}");
    Console.Write("команда: ");
    teams[i] = Console.ReadLine() ?? "";
    Console.Write("средняя скорость (км/ч): ");
    speeds[i] = double.Parse(Console.ReadLine() ?? "0");
    Console.WriteLine();
}

double sum = 0;
for (int i = 0; i < n; i++)
{
    sum += speeds[i];
}
double average = sum / n;

double maxSpeed = speeds[0];
double minSpeed = speeds[0];
string fastestTeam = teams[0];
string slowestTeam = teams[0];

for (int i = 1; i < n; i++)
{
    if (speeds[i] > maxSpeed)
    {
        maxSpeed = speeds[i];
        fastestTeam = teams[i];
    }
    if (speeds[i] < minSpeed)
    {
        minSpeed = speeds[i];
        slowestTeam = teams[i];
    }
}

Console.WriteLine("--- статистика квалификации ---");
Console.WriteLine($"средняя скорость: {average:F2} км/ч");
Console.WriteLine($"лидер: {fastestTeam} ({maxSpeed:F2} км/ч)");
Console.WriteLine($"самый медленный: {slowestTeam} ({minSpeed:F2} км/ч)");
Console.WriteLine($"разница темпа: {maxSpeed - minSpeed:F2} км/ч");
Console.WriteLine();

Console.WriteLine("--- исходный порядок ---");
Console.WriteLine("-----------------------------------------------");
Console.WriteLine("| команда                | скорость (км/ч) |");
Console.WriteLine("-----------------------------------------------");
for (int i = 0; i < n; i++)
{
    Console.WriteLine($"| {teams[i],-20} | {speeds[i],19:F2} |");
}
Console.WriteLine("-----------------------------------------------");
Console.WriteLine();

string[] sortedTeams = new string[n];
double[] sortedSpeeds = new double[n];
for (int i = 0; i < n; i++)
{
    sortedTeams[i] = teams[i];
    sortedSpeeds[i] = speeds[i];
}

for (int i = 0; i < n - 1; i++)
{
    for (int j = 0; j < n - i - 1; j++)
    {
        if (sortedSpeeds[j] < sortedSpeeds[j + 1])
        {
            double tempSpeed = sortedSpeeds[j];
            sortedSpeeds[j] = sortedSpeeds[j + 1];
            sortedSpeeds[j + 1] = tempSpeed;

            string tempTeam = sortedTeams[j];
            sortedTeams[j] = sortedTeams[j + 1];
            sortedTeams[j + 1] = tempTeam;
        }
    }
}

Console.WriteLine("--- итоговый протокол квалификации ---");
Console.WriteLine("-----------------------------------------------");
Console.WriteLine("| поз. | команда                | скорость     |");
Console.WriteLine("-----------------------------------------------");
for (int i = 0; i < n; i++)
{
    Console.WriteLine($"| {i + 1,4} | {sortedTeams[i],-20} | {sortedSpeeds[i],13:F2} |");
}
Console.WriteLine("-----------------------------------------------");
Console.WriteLine();

Console.WriteLine("--- дополнительно: фильтр по скорости ---");
Console.Write("введите минимальную скорость для отбора (км/ч): ");
double minFilter = double.Parse(Console.ReadLine() ?? "0");
Console.WriteLine();

Console.WriteLine($"команды со скоростью >= {minFilter:F2} км/ч:");
int count = 0;
for (int i = 0; i < n; i++)
{
    if (sortedSpeeds[i] >= minFilter)
    {
        Console.WriteLine($"- {sortedTeams[i]} ({sortedSpeeds[i]:F2} км/ч)");
        count++;
    }
}
Console.WriteLine();
Console.WriteLine($"отобрано команд: {count}");

Console.WriteLine();
Console.WriteLine("нажмите любую клавишу для выхода...");
Console.ReadKey();