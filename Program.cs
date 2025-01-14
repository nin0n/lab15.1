namespace lab15_1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    namespace FileWatcherExample
    {
        // Интерфейс наблюдателя
        public interface IObserver
        {
            void Update(string message);
        }

        // Конкретный наблюдатель
        public class ConsoleObserver : IObserver
        {
            public void Update(string message)
            {
                Console.WriteLine(message);
            }
        }

        // Класс, который будет отслеживать изменения (наблюдаемый объект)
        public class DirectoryWatcher
        {
            private readonly string _directoryPath;
            private readonly List<IObserver> _observers = new List<IObserver>();
            private Dictionary<string, DateTime> _fileStates;

            public DirectoryWatcher(string directoryPath)
            {
                _directoryPath = directoryPath;
                if (!Directory.Exists(_directoryPath))
                {
                    throw new DirectoryNotFoundException($"Директория {_directoryPath} не существует.");
                }

                // Инициализация текущего состояния файлов
                _fileStates = GetFileStates();
            }

            // Добавить наблюдателя
            public void Subscribe(IObserver observer)
            {
                _observers.Add(observer);
            }

            // Удалить наблюдателя
            public void Unsubscribe(IObserver observer)
            {
                _observers.Remove(observer);
            }

            // Уведомить всех наблюдателей
            private void NotifyObservers(string message)
            {
                foreach (var observer in _observers)
                {
                    observer.Update(message);
                }
            }

            // Получить текущее состояние файлов в директории
            private Dictionary<string, DateTime> GetFileStates()
            {
                return Directory.GetFiles(_directoryPath)
                                .ToDictionary(file => file, file => File.GetLastWriteTime(file));
            }

            // Проверить изменения в директории
            public void CheckForChanges()
            {
                var currentFileStates = GetFileStates();

                // Проверка на добавленные файлы
                foreach (var file in currentFileStates.Keys.Except(_fileStates.Keys))
                {
                    NotifyObservers($"Добавлен файл: {file}");
                }

                // Проверка на удаленные файлы
                foreach (var file in _fileStates.Keys.Except(currentFileStates.Keys))
                {
                    NotifyObservers($"Удален файл: {file}");
                }

                // Проверка на измененные файлы
                foreach (var file in _fileStates.Keys.Intersect(currentFileStates.Keys))
                {
                    if (_fileStates[file] != currentFileStates[file])
                    {
                        NotifyObservers($"Изменен файл: {file}");
                    }
                }

                // Обновление состояния
                _fileStates = currentFileStates;
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                Console.WriteLine("Введите путь к директории для наблюдения:");
                string directoryPath = Console.ReadLine();

                try
                {
                    // Создаем наблюдаемый объект
                    var watcher = new DirectoryWatcher(directoryPath);

                    // Добавляем наблюдателя
                    var consoleObserver = new ConsoleObserver();
                    watcher.Subscribe(consoleObserver);

                    Console.WriteLine("Начинаем наблюдение за изменениями... (нажмите Ctrl+C для выхода)");

                    // Периодически проверяем изменения
                    while (true)
                    {
                        watcher.CheckForChanges();
                        Thread.Sleep(5000); // Проверяем каждые 5 секунд
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}
