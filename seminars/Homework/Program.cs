using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Hw2_Variant4
{
    class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Faculty(int id, string name)
        {
            Id = id;
            Name = name ?? "";
        }

        public Faculty() : this(0, "") { }

        public override string ToString() => $"{Id}. {Name}";
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
                if (value < 0 || value > 5)
                    throw new ArgumentException("средний балл должен быть от 0 до 5");
                _gpa = value;
            }
        }

        public Student(int id, int facultyId, string name, double gpa)
        {
            Id = id;
            FacultyId = facultyId;
            Name = name ?? "";
            Gpa = gpa;
        }

        public Student() : this(0, 0, "", 0) { }

        public override string ToString() => $"{Id}. {Name} (балл: {Gpa:F2})";
    }

    class DatabaseManager
    {
        private string _connectionString;

        public DatabaseManager(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public void InitializeDatabase(string facultyCsvPath, string studentCsvPath)
        {
            CreateTables();
            if (GetAllFaculties().Count == 0 && File.Exists(facultyCsvPath))
            {
                ImportFacultiesFromCsv(facultyCsvPath);
                Console.WriteLine($"загружены факультеты из {facultyCsvPath}");
            }
            if (GetAllStudents().Count == 0 && File.Exists(studentCsvPath))
            {
                ImportStudentsFromCsv(studentCsvPath);
                Console.WriteLine($"загружены студенты из {studentCsvPath}");
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
                    student_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    faculty_id INTEGER NOT NULL,
                    student_name TEXT NOT NULL,
                    student_gpa REAL NOT NULL,
                    FOREIGN KEY (faculty_id) REFERENCES faculty(faculty_id)
                );";
            cmd.ExecuteNonQuery();
        }

        private void ImportFacultiesFromCsv(string path)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string[] lines = File.ReadAllLines(path);
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
        }

        private void ImportStudentsFromCsv(string path)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');
                if (parts.Length < 4) continue;
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO student (student_id, faculty_id, student_name, student_gpa)
                    VALUES (@id, @facultyId, @name, @gpa)";
                cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
                cmd.Parameters.AddWithValue("@facultyId", int.Parse(parts[1]));
                cmd.Parameters.AddWithValue("@name", parts[2]);
                cmd.Parameters.AddWithValue("@gpa", double.Parse(parts[3]));
                cmd.ExecuteNonQuery();
            }
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
            cmd.CommandText = "SELECT student_id, faculty_id, student_name, student_gpa FROM student ORDER BY student_id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Student(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDouble(3)));
            }
            return result;
        }

        public Student? GetStudentById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT student_id, faculty_id, student_name, student_gpa FROM student WHERE student_id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Student(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDouble(3));
            }
            return null;
        }

        public void AddStudent(Student student)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO student (faculty_id, student_name, student_gpa)
                VALUES (@facultyId, @name, @gpa)";
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
                SET faculty_id = @facultyId, student_name = @name, student_gpa = @gpa
                WHERE student_id = @id";
            cmd.Parameters.AddWithValue("@id", student.Id);
            cmd.Parameters.AddWithValue("@facultyId", student.FacultyId);
            cmd.Parameters.AddWithValue("@name", student.Name);
            cmd.Parameters.AddWithValue("@gpa", student.Gpa);
            cmd.ExecuteNonQuery();
        }

        public void DeleteStudent(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM student WHERE student_id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
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

        public List<Student> GetStudentsByFaculty(int facultyId)
        {
            var result = new List<Student>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT student_id, faculty_id, student_name, student_gpa
                FROM student WHERE faculty_id = @facultyId ORDER BY student_name";
            cmd.Parameters.AddWithValue("@facultyId", facultyId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Student(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDouble(3)));
            }
            return result;
        }

        public void ExportToCsv(string facultyPath, string studentPath)
        {
            var facultyLines = new List<string>();
            facultyLines.Add("faculty_id;faculty_name");
            foreach (var f in GetAllFaculties())
                facultyLines.Add($"{f.Id};{f.Name}");
            File.WriteAllLines(facultyPath, facultyLines.ToArray());

            var studentLines = new List<string>();
            studentLines.Add("student_id;faculty_id;student_name;student_gpa");
            foreach (var s in GetAllStudents())
                studentLines.Add($"{s.Id};{s.FacultyId};{s.Name};{s.Gpa:F2}");
            File.WriteAllLines(studentPath, studentLines.ToArray());
        }
    }

    class ReportBuilder
    {
        private DatabaseManager _db;
        private string _sql = "";
        private string _title = "";
        private string[] _headers = Array.Empty<string>();
        private int[] _widths = Array.Empty<int>();
        private bool _numbered = false;
        private string _footer = "";

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

        public ReportBuilder Numbered()
        {
            _numbered = true;
            return this;
        }

        public ReportBuilder Footer(string label)
        {
            _footer = label;
            return this;
        }

        public string Build()
        {
            var (columns, rows) = _db.ExecuteQuery(_sql);
            var sb = new StringBuilder();

            if (_title.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"=== {_title} ===");
            }

            string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
            int colCount = displayHeaders.Length;
            int[] widths;
            if (_widths.Length >= colCount)
                widths = _widths;
            else
            {
                widths = new int[colCount];
                for (int i = 0; i < colCount; i++)
                    widths[i] = 20;
            }

            if (_numbered)
                sb.Append($"{"№",-5}");

            for (int i = 0; i < colCount; i++)
            {
                sb.Append(displayHeaders[i].PadRight(widths[i]));
            }
            sb.AppendLine();

            int totalWidth = (_numbered ? 5 : 0);
            for (int i = 0; i < colCount; i++)
                totalWidth += widths[i];
            sb.AppendLine(new string('-', totalWidth));

            int rowNum = 1;
            foreach (var row in rows)
            {
                if (_numbered)
                    sb.Append($"{rowNum,-5}");
                rowNum++;
                for (int i = 0; i < colCount && i < row.Length; i++)
                {
                    sb.Append(row[i].PadRight(widths[i]));
                }
                sb.AppendLine();
            }

            if (_footer.Length > 0)
            {
                sb.AppendLine(new string('-', totalWidth));
                sb.AppendLine($"{_footer}: {rows.Count}");
            }

            return sb.ToString();
        }

        public void Print()
        {
            Console.WriteLine(Build());
        }

        public void SaveToFile(string path)
        {
            File.WriteAllText(path, Build());
            Console.WriteLine($"отчёт сохранён в {path}");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            string dbPath = "students.db";
            string facultyCsv = "faculty.csv";
            string studentCsv = "student.csv";

            var db = new DatabaseManager(dbPath);
            db.InitializeDatabase(facultyCsv, studentCsv);

            string choice;
            do
            {
                Console.WriteLine("\n=== УПРАВЛЕНИЕ СТУДЕНТАМИ ===");
                Console.WriteLine("1 - показать все факультеты");
                Console.WriteLine("2 - показать всех студентов");
                Console.WriteLine("3 - добавить студента");
                Console.WriteLine("4 - редактировать студента");
                Console.WriteLine("5 - удалить студента");
                Console.WriteLine("6 - отчёты");
                Console.WriteLine("7 - фильтр по факультету");
                Console.WriteLine("8 - экспорт в csv");
                Console.WriteLine("0 - выход");
                Console.Write("выбор: ");

                choice = Console.ReadLine()?.Trim() ?? "";
                Console.WriteLine();

                switch (choice)
                {
                    case "1": ShowFaculties(db); break;
                    case "2": ShowStudents(db); break;
                    case "3": AddStudent(db); break;
                    case "4": EditStudent(db); break;
                    case "5": DeleteStudent(db); break;
                    case "6": ReportsMenu(db); break;
                    case "7": FilterByFaculty(db); break;
                    case "8": ExportCsv(db); break;
                    case "0": Console.WriteLine("до свидания"); break;
                    default: Console.WriteLine("неверный пункт"); break;
                }
            } while (choice != "0");
        }

        static void ShowFaculties(DatabaseManager db)
        {
            Console.WriteLine("--- факультеты ---");
            foreach (var f in db.GetAllFaculties())
                Console.WriteLine(f);
        }

        static void ShowStudents(DatabaseManager db)
        {
            Console.WriteLine("--- студенты ---");
            foreach (var s in db.GetAllStudents())
                Console.WriteLine(s);
        }

        static void AddStudent(DatabaseManager db)
        {
            Console.WriteLine("--- добавление студента ---");
            Console.WriteLine("доступные факультеты:");
            foreach (var f in db.GetAllFaculties())
                Console.WriteLine(f);

            Console.Write("id факультета: ");
            if (!int.TryParse(Console.ReadLine(), out int facultyId))
            {
                Console.WriteLine("ошибка ввода");
                return;
            }

            Console.Write("имя студента: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (name == "")
            {
                Console.WriteLine("имя не может быть пустым");
                return;
            }

            Console.Write("средний балл (0-5): ");
            if (!double.TryParse(Console.ReadLine(), out double gpa))
            {
                Console.WriteLine("ошибка ввода");
                return;
            }

            try
            {
                var student = new Student(0, facultyId, name, gpa);
                db.AddStudent(student);
                Console.WriteLine("студент добавлен");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ошибка: {ex.Message}");
            }
        }

        static void EditStudent(DatabaseManager db)
        {
            Console.WriteLine("--- редактирование студента ---");
            Console.Write("id студента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ошибка ввода");
                return;
            }

            Student? student = db.GetStudentById(id);
            if (student == null)
            {
                Console.WriteLine("студент не найден");
                return;
            }

            Console.WriteLine($"текущие данные: {student}");

            Console.Write($"новое имя (enter - оставить '{student.Name}'): ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (name != "")
                student.Name = name;

            Console.Write($"новый балл (enter - оставить '{student.Gpa:F2}'): ");
            string gpaInput = Console.ReadLine()?.Trim() ?? "";
            if (gpaInput != "")
            {
                if (double.TryParse(gpaInput, out double gpa))
                {
                    try { student.Gpa = gpa; }
                    catch (ArgumentException ex) { Console.WriteLine($"ошибка: {ex.Message}"); return; }
                }
                else { Console.WriteLine("ошибка ввода"); return; }
            }

            Console.Write($"новый факультет (enter - оставить '{student.FacultyId}'): ");
            string facInput = Console.ReadLine()?.Trim() ?? "";
            if (facInput != "")
            {
                if (int.TryParse(facInput, out int facId))
                    student.FacultyId = facId;
                else { Console.WriteLine("ошибка ввода"); return; }
            }

            db.UpdateStudent(student);
            Console.WriteLine("данные обновлены");
        }

        static void DeleteStudent(DatabaseManager db)
        {
            Console.WriteLine("--- удаление студента ---");
            Console.Write("id студента: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ошибка ввода");
                return;
            }

            Student? student = db.GetStudentById(id);
            if (student == null)
            {
                Console.WriteLine("студент не найден");
                return;
            }

            Console.Write($"удалить '{student.Name}'? (да/нет): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (confirm == "да")
            {
                db.DeleteStudent(id);
                Console.WriteLine("студент удалён");
            }
            else
                Console.WriteLine("отменено");
        }

        static void ReportsMenu(DatabaseManager db)
        {
            string choice;
            do
            {
                Console.WriteLine("\n--- отчёты ---");
                Console.WriteLine("1 - студенты по факультетам");
                Console.WriteLine("2 - количество студентов по факультетам");
                Console.WriteLine("3 - средний балл по факультетам");
                Console.WriteLine("0 - назад");
                Console.Write("выбор: ");

                choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1": Report1(db); break;
                    case "2": Report2(db); break;
                    case "3": Report3(db); break;
                    case "0": break;
                    default: Console.WriteLine("неверный пункт"); break;
                }
            } while (choice != "0");
        }

        static void Report1(DatabaseManager db)
        {
            new ReportBuilder(db)
                .Query(@"SELECT s.student_name, f.faculty_name, s.student_gpa
                        FROM student s
                        JOIN faculty f ON s.faculty_id = f.faculty_id
                        ORDER BY s.student_name")
                .Title("студенты по факультетам")
                .Header("студент", "факультет", "балл")
                .ColumnWidths(25, 20, 10)
                .Numbered()
                .Footer("всего студентов")
                .Print();
        }

        static void Report2(DatabaseManager db)
        {
            new ReportBuilder(db)
                .Query(@"SELECT f.faculty_name, COUNT(*) as cnt
                        FROM student s
                        JOIN faculty f ON s.faculty_id = f.faculty_id
                        GROUP BY f.faculty_name
                        ORDER BY f.faculty_name")
                .Title("количество студентов по факультетам")
                .Header("факультет", "количество")
                .ColumnWidths(25, 10)
                .Print();
        }

        static void Report3(DatabaseManager db)
        {
            new ReportBuilder(db)
                .Query(@"SELECT f.faculty_name, ROUND(AVG(s.student_gpa), 2) as avg_gpa
                        FROM student s
                        JOIN faculty f ON s.faculty_id = f.faculty_id
                        GROUP BY f.faculty_name
                        ORDER BY avg_gpa DESC")
                .Title("средний балл по факультетам")
                .Header("факультет", "средний балл")
                .ColumnWidths(25, 15)
                .SaveToFile("report3.txt");
        }

        static void FilterByFaculty(DatabaseManager db)
        {
            Console.WriteLine("--- фильтр по факультету ---");
            Console.WriteLine("факультеты:");
            foreach (var f in db.GetAllFaculties())
                Console.WriteLine(f);

            Console.Write("id факультета: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ошибка ввода");
                return;
            }

            var students = db.GetStudentsByFaculty(id);
            if (students.Count == 0)
                Console.WriteLine("нет студентов на этом факультете");
            else
                foreach (var s in students)
                    Console.WriteLine(s);
        }

        static void ExportCsv(DatabaseManager db)
        {
            db.ExportToCsv("faculty_export.csv", "student_export.csv");
            Console.WriteLine("экспорт завершён: faculty_export.csv, student_export.csv");
        }
    }
}