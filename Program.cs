using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            var db = new NorthwindConsole_32_TMBContext();
            string userInput;
            do
            {
                Console.WriteLine("1) Display Categories");
                Console.WriteLine("2) Add Category");
                Console.WriteLine("3) Display Category and related Products");
                Console.WriteLine("4) Display all Categories related Products");
                Console.WriteLine("5) Add Product");
                Console.WriteLine("6) Edit Product");
                Console.WriteLine("7) Display all Products");
                Console.WriteLine("8) Display specific Product");
                Console.WriteLine("9) Edit Category");
                Console.WriteLine("\"q\" to quit");
                userInput = Console.ReadLine();
                Console.Clear();
                logger.Info($"Option {userInput} selected");

                //Display Categories
                if (userInput == "1")
                {
                    var query = db.Categories.OrderBy(p => p.CategoryName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{query.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    foreach (var item in query) { Console.WriteLine($"{item.CategoryName} - {item.Description}"); }
                    Console.ForegroundColor = ConsoleColor.White;
                }

                //Add Category
                else if (userInput == "2")
                {
                    Category category = new Category();
                    Console.WriteLine("Enter Category Name:");
                    category.CategoryName = Console.ReadLine();
                    Console.WriteLine("Enter the Category Description:");
                    category.Description = Console.ReadLine();

                    ValidationContext context = new ValidationContext(category, null, null);
                    List<ValidationResult> results = new List<ValidationResult>();

                    var isValid = Validator.TryValidateObject(category, context, results, true);
                    if (isValid)
                    {
                        if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                        {
                            isValid = false;
                            results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                        }
                        else
                        {
                            db.AddCategory(category);
                            logger.Info("Category added - {category.CategoryName}", category.CategoryName);
                        }
                    }
                    if (!isValid) { Console.Clear(); foreach (var result in results) { logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}"); } }
                }

                //Display Category and related Products
                else if (userInput == "3")
                {
                    var query = db.Categories.OrderBy(p => p.CategoryId);
                    Console.WriteLine("Select the category whose products you want to display:");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var item in query) { Console.WriteLine($"{item.CategoryId}) {item.CategoryName}"); }
                    Console.ForegroundColor = ConsoleColor.White;
                    try
                    {
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Product p in category.Products) { Console.WriteLine(p.ProductName); }
                    }
                    catch (FormatException) { logger.Error("Invalid input - CategoryID not selected"); }
                }

                //Display all Categories related Products
                else if (userInput == "4")
                {
                    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.CategoryName}");
                        foreach (Product p in item.Products) { Console.WriteLine($"\t{p.ProductName}"); }
                    }
                }

                //Add Product
                else if (userInput == "5")
                {
                    Product product = new Product();
                    var supplierId = db.Suppliers.OrderBy(p => p.SupplierId);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"{"SupplierId",-7}   {"CompanyName",7}");
                    foreach (var item in supplierId) { Console.WriteLine($"    {item.SupplierId,-1}         {item.CompanyName,-1}"); }
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine("Enter SupplierID:");
                    try
                    {
                        product.SupplierId = Int32.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"SupplierID {product.SupplierId} selected");
                        if (db.Suppliers.Any(c => c.SupplierId == product.SupplierId))
                        {
                            var categoryId = db.Categories.OrderBy(p => p.CategoryId);
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine($"{"CategoryID",-7}   {"CategoryName",7}");
                            foreach (var item in categoryId) { Console.WriteLine($"    {item.CategoryId,-1}         {item.CategoryName,-1}"); }
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.WriteLine("Enter CategoryID:");
                            try
                            {
                                product.CategoryId = Int32.Parse(Console.ReadLine());
                                Console.Clear();
                                logger.Info($"CategoryID {product.CategoryId} selected");
                                if (db.Categories.Any(c => c.CategoryId == product.CategoryId))
                                {
                                    Console.WriteLine("Enter Product Name:");
                                    product.ProductName = Console.ReadLine();

                                    ValidationContext context = new ValidationContext(product, null, null);
                                    List<ValidationResult> results = new List<ValidationResult>();

                                    var isValid = Validator.TryValidateObject(product, context, results, true);
                                    if (isValid)
                                    {
                                        if (db.Products.Any(c => c.ProductName == product.ProductName))
                                        {
                                            isValid = false;
                                            results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                                        }
                                        else
                                        {
                                            //might fill in later
                                            product.UnitPrice = null;
                                            product.QuantityPerUnit = null;
                                            product.UnitsInStock = null;
                                            product.UnitsOnOrder = null;
                                            product.ReorderLevel = null;
                                            product.Discontinued = false;
                                            Console.Clear();
                                            logger.Info("The rest of the fields where set to default values");
                                            db.AddProduct(product);
                                            logger.Info("Product added - {product.ProductName}", product.ProductName);
                                        }
                                    }
                                    if (!isValid) { Console.Clear(); foreach (var result in results) { logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}"); } }
                                }
                                else { logger.Error("Invalid SupplierID"); }
                            }
                            catch (FormatException) { Console.Clear(); logger.Error("Invalid input - CategoryID not selected"); }
                        }
                        else { logger.Error("Invalid CategoryID"); }
                    }
                    catch (FormatException) { Console.Clear(); logger.Error("Invalid input - SupplierID not selected"); }

                }

                //Edit Product
                else if (userInput == "6")
                {
                    Product product = new Product();

                    var productId = db.Products.OrderBy(p => p.ProductId);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"{"ProductId",-7}   {"ProductName",7}");
                    foreach (var item in productId) { Console.WriteLine($"    {item.ProductId,-1}         {item.ProductName,-1}"); }
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine("Enter ProductID:");
                    try
                    {
                        product.ProductId = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"ProductID {product.ProductId} selected");
                        string userInputEditProduct;
                        if (db.Products.Any(c => c.ProductId == product.ProductId))
                        {
                            product = db.Products.FirstOrDefault(c => c.ProductId == product.ProductId);

                            Console.WriteLine("Edit ProductName - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter ProductName:");
                                product.ProductName = Console.ReadLine();
                                Console.Clear();
                                logger.Info("ProductName updated");

                            }
                            else { Console.Clear(); }

                            //Edit SupplierID
                            Console.WriteLine("Edit SupplierID - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter SupplierID:");
                                try
                                {
                                    product.SupplierId = int.Parse(Console.ReadLine());
                                    Console.Clear();
                                    logger.Info("SupplierID updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - SupplierID did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit CategoryID
                            Console.WriteLine("Edit CategoryID - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter CategoryID:");
                                try
                                {
                                    product.CategoryId = int.Parse(Console.ReadLine());
                                    Console.Clear();
                                    logger.Info("CategoryID updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - CategoryID did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit QuantityPerUnit
                            Console.WriteLine("Edit QuantityPerUnit - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter QuantityPerUnit:");
                                try
                                {
                                    product.QuantityPerUnit = Console.ReadLine();
                                    Console.Clear();
                                    logger.Info("QuantityPerUnit updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - QuantityPerUnit did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit UnitPrice
                            Console.WriteLine("Edit UnitPrice - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter UnitPrice:");
                                try
                                {
                                    product.UnitPrice = Math.Round(decimal.Parse(Console.ReadLine()));
                                    Console.Clear();
                                    logger.Info("UnitPrice updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - UnitPrice did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit UnitsInStock
                            Console.WriteLine("Edit UnitsInStock - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter UnitsInStock:");
                                try
                                {
                                    product.UnitsInStock = short.Parse(Console.ReadLine());
                                    Console.Clear();
                                    logger.Info("UnitsInStock updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - UnitsInStock did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit UnitsOnOrder
                            Console.WriteLine("Edit UnitsOnOrder - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter UnitsOnOrder:");
                                try
                                {
                                    product.UnitsOnOrder = short.Parse(Console.ReadLine());
                                    Console.Clear();
                                    logger.Info("UnitsOnOrder updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - UnitsOnOrder did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit ReorderLevel
                            Console.WriteLine("Edit ReorderLevel - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                Console.WriteLine("Enter ReorderLevel:");
                                try
                                {
                                    product.ReorderLevel = short.Parse(Console.ReadLine());
                                    Console.Clear();
                                    logger.Info("ReorderLevel updated");
                                }
                                catch (FormatException) { Console.Clear(); logger.Error("Invalid input - ReorderLevel did not update"); }
                            }
                            else { Console.Clear(); }

                            //Edit Discontinued
                            Console.WriteLine("Edit Discontinued - y or n?");
                            userInputEditProduct = Console.ReadLine().ToLower();
                            if (userInputEditProduct.ToLower() == "y")
                            {
                                var query = db.Products.Where(p => p.ProductId == product.ProductId);
                                foreach (var item in query)
                                {
                                    if (item.Discontinued == false) { product.Discontinued = true; }
                                    else if (item.Discontinued == true) { product.Discontinued = false; }
                                }
                                logger.Info("Discontinued updated");
                            }
                            else { Console.Clear(); }
                            db.EditProduct(product);
                            logger.Info($"ProductID {product.ProductId} updated");
                        }
                        else { logger.Error("Invalid ProductID"); }
                    }
                    catch (FormatException) { Console.Clear(); logger.Error("Invalid input - ProductID not selected"); }
                }

                //Display all Products
                else if (userInput == "7")
                {
                    string userInputProductDisplay;
                    Console.WriteLine("1) Display Products");
                    Console.WriteLine("2) Display discontinued Products");
                    Console.WriteLine("3) Display acitve Products");
                    Console.WriteLine("Enter any other key to quit to previous menu");
                    userInputProductDisplay = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {userInput} selected");

                    //Display all Products
                    if (userInputProductDisplay == "1")
                    {
                        var query = db.Products.OrderBy(p => p.ProductName);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query) { Console.WriteLine($"{item.ProductName}"); }
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    //Display discontinued Products
                    if (userInputProductDisplay == "2")
                    {
                        var query = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductName);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query) { Console.WriteLine($"{item.ProductName}"); }
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    //Display active Products
                    if (userInputProductDisplay == "3")
                    {
                        var query = db.Products.Where(p => p.Discontinued == false).OrderBy(p => p.ProductName);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query) { Console.WriteLine($"{item.ProductName}"); }
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }

                //Display specific Product
                else if (userInput == "8")
                {
                    Console.WriteLine("Enter ProductID");
                    Console.ForegroundColor = ConsoleColor.White;
                    try
                    {
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"ProductID {id} selected");
                        var query = db.Products.Where(p => p.ProductId == id);
                        foreach (var item in query) { Console.WriteLine($"ProductID: {item.ProductId}\nProductName: {item.ProductName}\nSupplierID: {item.SupplierId}\nCategoryID: {item.CategoryId}\nQuantityPerUnit: {item.QuantityPerUnit}\nUnitPrice: {item.UnitPrice,0:C}\nUnitsInStock: {item.UnitsInStock}\nUnitsOnOrder: {item.UnitsOnOrder}\nReorderLevel: {item.ReorderLevel}\nDiscontinued: {item.Discontinued}"); }
                    }
                    catch (FormatException) { Console.Clear(); logger.Error("Invalid input - ProductID not selected"); }
                }

                //Edit Category
                else if (userInput == "9")
                {
                    Category category = new Category();

                    var categoryId = db.Categories.OrderBy(p => p.CategoryId);
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"{"CategoryID",-7}   {"CategoryName",7}");
                    foreach (var item in categoryId) { Console.WriteLine($"    {item.CategoryId,-1}         {item.CategoryName,-1}"); }
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine("Enter CategoryID:");
                    try
                    {
                        category.CategoryId = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryID {category.CategoryId} selected");
                        string userInputEditCategory;
                        if (db.Categories.Any(c => c.CategoryId == category.CategoryId))
                        {
                            category = db.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);

                            //Edit CategoryName
                            Console.WriteLine("Edit CategoryName - y or n?");
                            userInputEditCategory = Console.ReadLine().ToLower();
                            if (userInputEditCategory.ToLower() == "y")
                            {
                                Console.WriteLine("Enter CategoryName:");
                                category.CategoryName = Console.ReadLine();
                                Console.Clear();
                                logger.Info("CategoryName updated");
                            }
                            else { Console.Clear(); }

                            //Edit CategoryDescription
                            Console.WriteLine("Edit CategoryDescription - y or n?");
                            userInputEditCategory = Console.ReadLine().ToLower();
                            if (userInputEditCategory.ToLower() == "y")
                            {
                                Console.WriteLine("Enter CategoryDescription:");
                                category.Description = Console.ReadLine();
                                Console.Clear();
                                logger.Info("CategoryName updated");
                            }
                            else { Console.Clear(); }
                            db.EditCategory(category);
                            logger.Info($"CategoryID {category.CategoryId} updated");
                        }
                        else { logger.Error("Invalid CategoryID"); }
                    }
                    catch (FormatException) { Console.Clear(); logger.Error("Invalid input - CategoryID not selected"); }
                }

                Console.WriteLine();

            } while (userInput.ToLower() != "q");
            logger.Info("Program ended");
        }
    }
}