namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            ImportedObjects = new List<ImportedObject>() { new ImportedObject() };

            var streamReader = new StreamReader(fileToImport);

            var importedLines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                var importedObject = new ImportedObject();
                importedObject.Type = values.Length >= 1 ? values[0] : String.Empty;
                importedObject.Name = values.Length >= 2 ? values[1] : String.Empty;
                importedObject.Schema = values.Length >= 3 ? values[2] : String.Empty;
                importedObject.ParentName = values.Length >= 4 ? values[3] : String.Empty;
                importedObject.ParentType = values.Length >= 5 ? values[4] : String.Empty;
                importedObject.DataType = values.Length >= 6 ? values[5] : String.Empty;
                importedObject.IsNullable = values.Length >= 7 ? values[6] : String.Empty;
                ((List<ImportedObject>)ImportedObjects).Add(importedObject);
            }

            // clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                if (!String.IsNullOrEmpty(importedObject.Type))
                {
                    importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                }

                if (!String.IsNullOrEmpty(importedObject.Name))
                {
                    importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }

                if (!String.IsNullOrEmpty(importedObject.Schema))
                {
                    importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }

                if (!String.IsNullOrEmpty(importedObject.ParentName))
                {
                    importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }

                if (!String.IsNullOrEmpty(importedObject.ParentType))
                {
                    importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type)
                    {
                        if (impObj.ParentName == importedObject.Name)
                        {
                            importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                        }
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType?.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType?.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren { get; set; }
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
