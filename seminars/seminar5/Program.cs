using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Seminar5_DependencyInjection
{
    interface ILogger
    {
        void Log(string message);
    }

    interface IBookStorage
    {
        void Save(string title, string author);
    }

    class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[log] {message}");
        }
    }

    class FileLogger : ILogger
    {
        private string _filePath;
        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }
        public void Log(string message)
        {
            File.AppendAllText(_filePath, $"[log] {message}\n");
        }
    }

    class NullLogger : ILogger
    {
        public void Log(string message) { }
    }

    class InMemoryBookStorage : IBookStorage
    {
        private ILogger _logger;
        private List<string> _books = new List<string>();

        public InMemoryBookStorage(ILogger logger)
        {
            _logger = logger;
        }

        public void Save(string title, string author)
        {
            _books.Add($"{title} - {author}");
            _logger.Log($"[storage] сохранено: {title}");
        }
    }

    class NaiveConsoleLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[log] {message}");
        }
    }

    class NaiveBookCatalogService
    {
        private NaiveConsoleLogger _logger = new NaiveConsoleLogger();

        public void AddBook(string title, string author)
        {
            _logger.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            _logger.Log($"удалена книга: {title}");
        }
    }

    class NaiveFileLogger
    {
        private string _fileName;
        public NaiveFileLogger(string fileName)
        {
            _fileName = fileName;
        }
        public void Log(string message)
        {
            File.AppendAllText(_fileName, $"[log] {message}\n");
        }
    }

    class NaiveBookCatalogServiceWithFlag
    {
        private bool _useFile;
        public NaiveBookCatalogServiceWithFlag(bool useFile = false)
        {
            _useFile = useFile;
        }

        public void AddBook(string title, string author)
        {
            if (_useFile)
                File.AppendAllText("log.txt", $"добавлена книга: {title}\n");
            else
                Console.WriteLine($"[log] добавлена книга: {title}");
        }
    }

    class BookCatalogServiceConstructor
    {
        private ILogger _logger;

        public BookCatalogServiceConstructor(ILogger logger)
        {
            _logger = logger;
        }

        public void AddBook(string title, string author)
        {
            _logger.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            _logger.Log($"удалена книга: {title}");
        }
    }

    class BookCatalogServiceProperty
    {
        public ILogger Logger { get; set; } = new NullLogger();

        public void AddBook(string title, string author)
        {
            Logger.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            Logger.Log($"удалена книга: {title}");
        }
    }

    class BookCatalogServiceMethod
    {
        public void AddBook(string title, string author, ILogger logger)
        {
            logger.Log($"добавлена книга: {title} - {author}");
        }
    }

    class BookCatalogService
    {
        private ILogger _logger;
        private IBookStorage _storage;

        public BookCatalogService(ILogger logger, IBookStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public void AddBook(string title, string author)
        {
            _storage.Save(title, author);
            _logger.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            _logger.Log($"удалена книга: {title}");
        }
    }

    class ServiceLocator
    {
        private static ServiceProvider? _provider;
        public static void Init(ServiceProvider provider) { _provider = provider; }
        public static T Get<T>() where T : notnull
        {
            if (_provider == null)
                throw new InvalidOperationException("service locator не инициализирован");
            return _provider.GetRequiredService<T>();
        }
    }

    class BookCatalogServiceAntiSL
    {
        public void AddBook(string title, string author)
        {
            ILogger logger = ServiceLocator.Get<ILogger>();
            IBookStorage storage = ServiceLocator.Get<IBookStorage>();
            storage.Save(title, author);
            logger.Log($"добавлена книга: {title} - {author}");
        }
    }

    static class LoggerContext
    {
        public static ILogger Current { get; set; } = new NullLogger();
    }

    class BookCatalogServiceAntiAC
    {
        public void AddBook(string title, string author)
        {
            LoggerContext.Current.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            LoggerContext.Current.Log($"удалена книга: {title}");
        }
    }

    class BookCatalogServiceAntiBI
    {
        private ILogger _logger;

        public BookCatalogServiceAntiBI() : this(new ConsoleLogger())
        {
        }

        public BookCatalogServiceAntiBI(ILogger logger)
        {
            _logger = logger;
        }

        public void AddBook(string title, string author)
        {
            _logger.Log($"добавлена книга: {title} - {author}");
        }
    }

    class CorrectBookCatalogService
    {
        private ILogger _logger;
        private IBookStorage _storage;

        public CorrectBookCatalogService(ILogger logger, IBookStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public void AddBook(string title, string author)
        {
            _storage.Save(title, author);
            _logger.Log($"добавлена книга: {title} - {author}");
        }

        public void RemoveBook(string title)
        {
            _logger.Log($"удалена книга: {title}");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== 1. наивная реализация ===");
            NaiveBookCatalogService naive = new NaiveBookCatalogService();
            naive.AddBook("евгений онегин", "пушкин");
            naive.RemoveBook("евгений онегин");

            Console.WriteLine("\n=== 2. внедрение через конструктор ===");
            BookCatalogServiceConstructor s1 = new BookCatalogServiceConstructor(new ConsoleLogger());
            s1.AddBook("евгений онегин", "пушкин");
            BookCatalogServiceConstructor s2 = new BookCatalogServiceConstructor(new FileLogger("log.txt"));
            s2.AddBook("сборник стихотворений", "пушкин");

            Console.WriteLine("\n=== 3. внедрение через свойство ===");
            BookCatalogServiceProperty s3 = new BookCatalogServiceProperty();
            s3.AddBook("евгений онегин", "пушкин");
            s3.Logger = new ConsoleLogger();
            s3.AddBook("сборник стихотворений", "пушкин");

            Console.WriteLine("\n=== 4. внедрение через параметр метода ===");
            BookCatalogServiceMethod s4 = new BookCatalogServiceMethod();
            s4.AddBook("евгений онегин", "пушкин", new ConsoleLogger());
            s4.AddBook("сборник стихотворений", "пушкин", new FileLogger("audit.log"));

            Console.WriteLine("\n=== 5. pure di (точка сборки) ===");
            ILogger logger = new ConsoleLogger();
            IBookStorage storage = new InMemoryBookStorage(logger);
            BookCatalogService s5 = new BookCatalogService(logger, storage);
            s5.AddBook("евгений онегин", "пушкин");
            s5.AddBook("сборник стихотворений", "пушкин");

            Console.WriteLine("\n=== 6. di контейнер microsoft.extensions.dependencyinjection ===");
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<IBookStorage, InMemoryBookStorage>();
            services.AddTransient<BookCatalogService>();

            ServiceProvider provider = services.BuildServiceProvider(
                new ServiceProviderOptions { ValidateOnBuild = true });

            BookCatalogService s6 = provider.GetRequiredService<BookCatalogService>();
            s6.AddBook("евгений онегин", "пушкин");
            s6.AddBook("сборник стихотворений", "пушкин");

            Console.WriteLine("\n=== 7. жизненные циклы (scoped) ===");
            ServiceCollection scopedServices = new ServiceCollection();
            scopedServices.AddScoped<IBookStorage, InMemoryBookStorage>();
            scopedServices.AddSingleton<ILogger, ConsoleLogger>();

            ServiceProvider rootProvider = scopedServices.BuildServiceProvider();

            using (IServiceScope scope1 = rootProvider.CreateScope())
            {
                IBookStorage storage1 = scope1.ServiceProvider.GetRequiredService<IBookStorage>();
                IBookStorage storage2 = scope1.ServiceProvider.GetRequiredService<IBookStorage>();
                Console.WriteLine($"одна область: storage1 == storage2 = {ReferenceEquals(storage1, storage2)}");
            }

            using (IServiceScope scope2 = rootProvider.CreateScope())
            {
                IBookStorage storage3 = scope2.ServiceProvider.GetRequiredService<IBookStorage>();
                IBookStorage storage4 = scope2.ServiceProvider.GetRequiredService<IBookStorage>();
                Console.WriteLine($"другая область: storage3 == storage4 = {ReferenceEquals(storage3, storage4)}");
            }

            Console.WriteLine("\n=== 8. антипаттерны ===");
            Console.WriteLine("8.1 control freak - класс сам создаёт зависимости");
            Console.WriteLine("8.2 bastard injection - два конструктора");
            Console.WriteLine("8.3 service locator - глобальный реестр");
            Console.WriteLine("8.4 ambient context - глобальное статическое состояние");

            Console.WriteLine("\n=== 9. исправление антипаттернов (контрольное задание) ===");
            ILogger correctLogger = new ConsoleLogger();
            IBookStorage correctStorage = new InMemoryBookStorage(correctLogger);
            CorrectBookCatalogService correctService = new CorrectBookCatalogService(correctLogger, correctStorage);
            correctService.AddBook("евгений онегин", "пушкин");
            correctService.RemoveBook("евгений онегин");
        }
    }
}