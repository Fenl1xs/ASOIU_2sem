using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

class Faculty
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Faculty(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public Faculty() : this(0, "") { }

    public override string ToString()
    {
        return Id + ". " + Name;
    }
}

class Student
{
    public int Id { get; set; }
    public int FacultyId { get; set; }
    public string Name { get; set; }

    private double _gpa;
    public double Gpa
    {
        get => _gpa;
        set
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Балл должен быть от 1 до 5");
            _gpa = value;
        }
    }

    public Student(int id, int facultyId, string name, double gpa)
    {
        Id = id;
        FacultyId = facultyId;
        Name = name;
        Gpa = gpa;
    }

    public Student() : this(0, 0, "", 0) { }

    public override string ToString()
    {
        return Id + "; " + Name + "; ID факультета: " + FacultyId + "; " + Gpa;
    }
}

class DatabaseManager
{
    private string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = "Data Source=" + dbPath;
    }

    public void InitializeDatabase(string facultiesCsvPath, string studentsCsvPath)
    {
        CreateTables();

        if (GetAllFaculties().Count == 0 && File.Exists(facultiesCsvPath))
        {
            ImportFacultiesFromCsv(facultiesCsvPath);
        }

        if (GetAllStudents().Count == 0 && File.Exists(studentsCsvPath))
        {
            ImportStudentsFromCsv(studentsCsvPath);
        }
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS faculty (
                faculty_id INTEGER PRIMARY KEY AUTOINCREMENT,
                faculty_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS student (
                student_id INTEGER PRIMARY KEY,
                faculty_id INTEGER NOT NULL,
                student_name TEXT NOT NULL,
                gpa REAL NOT NULL,
                FOREIGN KEY (faculty_id) REFERENCES faculty(faculty_id)
            );";
        cmd.ExecuteNonQuery();
    }

    private void ImportFacultiesFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO faculty (faculty_id, faculty_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Загружены факультеты");
    }

    private void ImportStudentsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO student (student_id, faculty_id, student_name, gpa)
                VALUES (@id, @facultyId, @name, @gpa)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@facultyId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@gpa", double.Parse(parts[3], CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Загружены студенты");
    }

    public List<Faculty> GetAllFaculties()
    {
        var result = new List<Faculty>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT faculty_id, faculty_name FROM faculty ORDER BY faculty_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Faculty(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Student> GetAllStudents()
    {
        var result = new List<Student>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT student_id, faculty_id, student_name, gpa FROM student ORDER BY student_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Student(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        }
        return result;
    }

    public Student GetStudentById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT student_id, faculty_id, student_name, gpa FROM student WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Student(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3));
        }
        return null;
    }

    public int GetNextStudentId()
    {
        var students = GetAllStudents();
        if (students.Count == 0) return 1;
        int maxId = 0;
        foreach (var s in students)
        {
            if (s.Id > maxId) maxId = s.Id;
        }
        return maxId + 1;
    }

    public void AddStudent(Student student)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO student (student_id, faculty_id, student_name, gpa)
            VALUES (@id, @facultyId, @name, @gpa)";
        student.Id = GetNextStudentId();
        cmd.Parameters.AddWithValue("@id", student.Id);
        cmd.Parameters.AddWithValue("@facultyId", student.FacultyId);
        cmd.Parameters.AddWithValue("@name", student.Name);
        cmd.Parameters.AddWithValue("@gpa", student.Gpa);
        cmd.ExecuteNonQuery();
    }

    public void UpdateStudent(Student student)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE student
            SET faculty_id = @facultyId, student_name = @name, gpa = @gpa
            WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", student.Id);
        cmd.Parameters.AddWithValue("@facultyId", student.FacultyId);
        cmd.Parameters.AddWithValue("@name", student.Name);
        cmd.Parameters.AddWithValue("@gpa", student.Gpa);
        cmd.ExecuteNonQuery();
    }

    private void RenumberStudents()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var students = new List<(int facultyId, string name, double gpa)>();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT faculty_id, student_name, gpa FROM student ORDER BY student_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            students.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2)));
        }

        cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM student";
        cmd.ExecuteNonQuery();

        for (int i = 0; i < students.Count; i++)
        {
            var s = students[i];
            cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO student (student_id, faculty_id, student_name, gpa)
                VALUES (@id, @facultyId, @name, @gpa)";
            cmd.Parameters.AddWithValue("@id", i + 1);
            cmd.Parameters.AddWithValue("@facultyId", s.facultyId);
            cmd.Parameters.AddWithValue("@name", s.name);
            cmd.Parameters.AddWithValue("@gpa", s.gpa);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteStudent(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM student WHERE student_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        RenumberStudents();
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }
}

class ReportBuilder
{
    private DatabaseManager _db;
    private string _sql = "";
    private string _title = "";
    private string[] _headers = Array.Empty<string>();
    private int[] _widths = Array.Empty<int>();

    public ReportBuilder(DatabaseManager db)
    {
        _db = db;
    }

    public ReportBuilder Query(string sql)
    {
        _sql = sql;
        return this;
    }

    public ReportBuilder Title(string title)
    {
        _title = title;
        return this;
    }

    public ReportBuilder Header(params string[] columns)
    {
        _headers = columns;
        return this;
    }

    public ReportBuilder ColumnWidths(params int[] widths)
    {
        _widths = widths;
        return this;
    }

    public string Build()
    {
        var (columns, rows) = _db.ExecuteQuery(_sql);
        var sb = new StringBuilder();

        if (_title.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("=== " + _title + " ===");
        }

        string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
        int colCount = displayHeaders.Length;
        int[] widths;

        if (_widths.Length >= colCount)
        {
            widths = _widths;
        }
        else
        {
            widths = new int[colCount];
            for (int i = 0; i < colCount; i++)
                widths[i] = 20;
        }

        for (int i = 0; i < colCount; i++)
            sb.Append(displayHeaders[i].PadRight(widths[i]));
        sb.AppendLine();

        int totalWidth = 0;
        for (int i = 0; i < colCount; i++)
            totalWidth += widths[i];
        sb.AppendLine(new string('-', totalWidth));

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < rows[r].Length && c < colCount; c++)
                sb.Append(rows[r][c].PadRight(widths[c]));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public void Print()
    {
        Console.WriteLine(Build());
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string dbPath = "university.db";
        string facultiesCsv = Path.Combine(AppContext.BaseDirectory, "faculties.csv");
        string studentsCsv = Path.Combine(AppContext.BaseDirectory, "students.csv");

        var db = new DatabaseManager(dbPath);
        db.InitializeDatabase(facultiesCsv, studentsCsv);

        Console.WriteLine();

        string choice;
        do
        {
            Console.WriteLine();
            Console.WriteLine("Управление:");
            Console.WriteLine("1. Показать все факультеты");
            Console.WriteLine("2. Показать всех студентов");
            Console.WriteLine("3. Добавить студента");
            Console.WriteLine("4. Редактировать студента");
            Console.WriteLine("5. Удалить студента");
            Console.WriteLine("6. Отчеты");
            Console.WriteLine("0. Выход");
            Console.Write("Ваш выбор: ");

            choice = Console.ReadLine();

            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1": ShowFaculties(db); break;
                    case "2": ShowStudents(db); break;
                    case "3": AddStudent(db); break;
                    case "4": EditStudent(db); break;
                    case "5": DeleteStudent(db); break;
                    case "6": ReportsMenu(db); break;
                    case "0": Console.WriteLine("До свидания!"); break;
                    default: Console.WriteLine("Неверный пункт."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }

        } while (choice != "0");
    }

    static void ShowFaculties(DatabaseManager db)
    {
        Console.WriteLine("Все факультеты:");
        var faculties = db.GetAllFaculties();
        foreach (var f in faculties)
            Console.WriteLine("  " + f);
        Console.WriteLine("Всего факультетов: " + faculties.Count);
    }

    static void ShowStudents(DatabaseManager db)
    {
        Console.WriteLine("Все студенты:");
        var students = db.GetAllStudents();
        foreach (var s in students)
            Console.WriteLine("  " + s);
        Console.WriteLine("Всего студентов: " + students.Count);
    }

    static void AddStudent(DatabaseManager db)
    {
        Console.WriteLine("Добавление студента:");
        Console.WriteLine();

        Console.WriteLine("Доступные факультеты:");
        var faculties = db.GetAllFaculties();
        foreach (var f in faculties)
            Console.WriteLine("  " + f);
        Console.WriteLine();

        Console.Write("Введите ID факультета: ");
        if (!int.TryParse(Console.ReadLine(), out int facultyId))
        {
            Console.WriteLine("Ошибка: нужно ввести число.");
            return;
        }

        bool facultyExists = false;
        foreach (var f in faculties)
        {
            if (f.Id == facultyId)
            {
                facultyExists = true;
                break;
            }
        }

        if (!facultyExists)
        {
            Console.WriteLine("Ошибка: факультет с ID " + facultyId + " не найден.");
            return;
        }

        Console.Write("Введите имя студента: ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Ошибка: имя не может быть пустым.");
            return;
        }

        Console.Write("Введите средний балл (от 1 до 5): ");
        if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double gpa))
        {
            Console.WriteLine("Ошибка: нужно ввести число.");
            return;
        }

        try
        {
            var student = new Student(0, facultyId, name.Trim(), gpa);
            db.AddStudent(student);
            Console.WriteLine("Студент успешно добавлен! Его ID: " + student.Id);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static void EditStudent(DatabaseManager db)
    {
        Console.WriteLine("Редактирование студента");
        Console.WriteLine();

        Console.Write("Введите ID студента: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Ошибка: нужно ввести число.");
            return;
        }

        var student = db.GetStudentById(id);
        if (student == null)
        {
            Console.WriteLine("Студент с ID " + id + " не найден.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Текущие данные:");
        Console.WriteLine("  " + student);
        Console.WriteLine();
        Console.WriteLine("(оставьте поле пустым и нажмите Enter, чтобы не менять значение)");
        Console.WriteLine();

        Console.Write("Новое имя (" + student.Name + "): ");
        string input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
            student.Name = input.Trim();

        Console.Write("Новый ID факультета (" + student.FacultyId + "): ");
        input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (int.TryParse(input, out int newFacultyId))
            {
                var faculties = db.GetAllFaculties();
                bool facultyExists = false;
                foreach (var f in faculties)
                {
                    if (f.Id == newFacultyId)
                    {
                        facultyExists = true;
                        break;
                    }
                }

                if (!facultyExists)
                {
                    Console.WriteLine("Ошибка: факультет с ID " + newFacultyId + " не найден.");
                    return;
                }

                student.FacultyId = newFacultyId;
            }
            else
            {
                Console.WriteLine("Ошибка: нужно ввести число.");
                return;
            }
        }

        Console.Write("Новый средний балл (" + student.Gpa + "): ");
        input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double newGpa))
            {
                try
                {
                    student.Gpa = newGpa;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine("Ошибка: нужно ввести число.");
                return;
            }
        }

        db.UpdateStudent(student);
        Console.WriteLine("Данные успешно обновлены!");
    }

    static void DeleteStudent(DatabaseManager db)
    {
        Console.WriteLine("Удаление студента");
        Console.WriteLine();

        Console.Write("Введите ID студента: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Ошибка: нужно ввести число.");
            return;
        }

        var student = db.GetStudentById(id);
        if (student == null)
        {
            Console.WriteLine("Студент с ID " + id + " не найден.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Вы собираетесь удалить:");
        Console.WriteLine("  " + student);
        Console.WriteLine();

        Console.Write("Вы уверены? (y/n): ");
        string confirm = Console.ReadLine();

        if (confirm == "y" || confirm == "Y" || confirm == "yes" || confirm == "YES")
        {
            db.DeleteStudent(id);
            Console.WriteLine("Студент удален.");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }
    }

    static void ReportsMenu(DatabaseManager db)
    {
        string choice;
        do
        {
            Console.WriteLine();
            Console.WriteLine("Отчеты");
            Console.WriteLine("1. Студенты по факультетам");
            Console.WriteLine("2. Количество студентов на факультетах");
            Console.WriteLine("3. Средний балл по факультетам");
            Console.WriteLine("0. Назад");
            Console.Write("Ваш выбор: ");

            choice = Console.ReadLine();

            Console.WriteLine();

            switch (choice)
            {
                case "1": Report1_StudentsWithFaculties(db); break;
                case "2": Report2_CountByFaculty(db); break;
                case "3": Report3_AvgGpaByFaculty(db); break;
                case "0": break;
                default: Console.WriteLine("Неверный пункт меню."); break;
            }

        } while (choice != "0");
    }

    static void Report1_StudentsWithFaculties(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"
                SELECT s.student_name, f.faculty_name, s.gpa
                FROM student s
                JOIN faculty f ON s.faculty_id = f.faculty_id
                ORDER BY s.student_id")
            .Title("Список студентов по факультетам")
            .Header("Имя студента", "Факультет", "Балл")
            .ColumnWidths(25, 30, 10)
            .Print();
    }

    static void Report2_CountByFaculty(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"
                SELECT f.faculty_name, COUNT(*) AS students_count
                FROM student s
                JOIN faculty f ON s.faculty_id = f.faculty_id
                GROUP BY f.faculty_name
                ORDER BY f.faculty_name")
            .Title("Количество студентов на факультетах")
            .Header("Факультет", "Кол-во студентов")
            .ColumnWidths(35, 20)
            .Print();
    }

    static void Report3_AvgGpaByFaculty(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"
            SELECT f.faculty_name, ROUND(AVG(s.gpa), 2) AS avg_gpa
            FROM student s
            JOIN faculty f ON s.faculty_id = f.faculty_id
            GROUP BY f.faculty_name
            ORDER BY avg_gpa DESC")
            .Title("Средний балл по факультетам")
            .Header("Факультет", "Средний балл")
            .ColumnWidths(35, 15)
            .Print();
    }
}