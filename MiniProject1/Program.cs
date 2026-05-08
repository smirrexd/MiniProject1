using System.Text.RegularExpressions;

var products = new List<string>();

Console.WriteLine("====================================");
Console.WriteLine("Welcome to the Product List Manager!");
Console.WriteLine("====================================\n");

while (true)
{
    ShowMenu();
    Console.Write("Choose an option: ");
    var choice = Console.ReadLine();
    if (choice is null) break;

    choice = choice.Trim();
    if (string.Equals(choice, "exit", StringComparison.OrdinalIgnoreCase))
        break;

    switch (choice)
    {
        case "1":
            AddProduct();
            Console.WriteLine("\n------------------------------------");
            break;
        case "2":
            ViewProducts();
            Console.WriteLine("\n------------------------------------");
            break;
        case "3":
            SearchProduct();
            Console.WriteLine("\n------------------------------------");
            break;
        case "4":
            DeleteProduct();
            Console.WriteLine("\n------------------------------------");
            break;
        case "5":
            ShowStatistics();
            Console.WriteLine("\n------------------------------------");
            break;
        case "6":
            goto ExitApp;
        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid option. Enter a number between 1 and 6.");
            Console.ResetColor();
            break;
    }

    Console.WriteLine();
}

ExitApp:
Console.WriteLine("Exiting application. Press any key to close...");
Console.ReadKey(true);


void ShowMenu()
{
    Console.WriteLine("Menu:");
    Console.WriteLine(" 1. Add product");
    Console.WriteLine(" 2. View products");
    Console.WriteLine(" 3. Search product");
    Console.WriteLine(" 4. Delete product");
    Console.WriteLine(" 5. Statistics");
    Console.WriteLine(" 6. Exit");
}

void AddProduct()
{
    Console.WriteLine();
    Console.WriteLine("Add product (format: LETTERS-NUMBER, e.g. ABC-200). Type 'exit' to return to menu.");
    while (true)
    {
        Console.Write("Enter product name: ");
        var input = Console.ReadLine();
        if (input is null) return;

        var trimmed = input.Trim();
        if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase))
            return;

        if (!TryValidateAndNormalize(trimmed, out var normalized, out var error))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            continue;
        }

        // duplicate check (case-insensitive)
        if (products.Any(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Product already added. Duplicate entries are not allowed.");
            Console.ResetColor();
            continue;
        }

        products.Add(normalized);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Added: '{normalized}' (total: {products.Count})");
        Console.ResetColor();
        return;
    }
}

void ViewProducts()
{
    Console.WriteLine();
    if (products.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No products were entered.");
        Console.ResetColor();
        return;
    }
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Products entered ({products.Count}):");
    for (int i = 0; i < products.Count; i++)
        Console.WriteLine($"{i + 1}. {products[i]}");
    Console.ResetColor();
}

void SearchProduct()
{
    Console.WriteLine();
    Console.WriteLine("Search products. Enter a search term (partial or full). Type 'exit' to return to menu.");
    Console.Write("Search: ");
    var term = Console.ReadLine();
    if (term is null) return;
    term = term.Trim();
    if (string.Equals(term, "exit", StringComparison.OrdinalIgnoreCase)) return;

    var matches = products.Where(p => p.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
    if (matches.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No products match the search term.");
        Console.ResetColor();
        return;
    }
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Found {matches.Count} match(es):");
    for (int i = 0; i < matches.Count; i++)
        Console.WriteLine($"{i + 1}. {matches[i]}");
    Console.ResetColor();
}

void DeleteProduct()
{
    Console.WriteLine();
    if (products.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("No products to delete.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine("Delete product: Enter the exact product code to delete (e.g. ABC-200). Type 'exit' to return to menu.");
    Console.Write("Product to delete: ");
    var input = Console.ReadLine();
    if (input is null) return;
    var trimmed = input.Trim();
    if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase)) return;

    // Normalize input if valid format, otherwise try to match as-is
    if (TryValidateAndNormalize(trimmed, out var normalized, out _))
    {
        var index = products.FindIndex(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            Console.Write($"Delete {products[index]}? (y/n): ");
            var conf = Console.ReadLine();
            if (conf != null && conf.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                products.RemoveAt(index);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Product deleted.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
            return;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Product not found.");
            Console.ResetColor();
            return;
        }
    }

    // If input wasn't valid product format, try partial search and offer deletions
    var matches = products.Where(p => p.Contains(trimmed, StringComparison.OrdinalIgnoreCase)).ToList();
    if (matches.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("No products match the input.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine($"Found {matches.Count} match(es):");
    for (int i = 0; i < matches.Count; i++)
        Console.WriteLine($"{i + 1}. {matches[i]}");

    Console.Write("Enter the number of the product to delete (or 'exit' to cancel): ");
    var sel = Console.ReadLine();
    if (sel is null) return;
    sel = sel.Trim();
    if (string.Equals(sel, "exit", StringComparison.OrdinalIgnoreCase)) return;

    if (int.TryParse(sel, out var selNum) && selNum >= 1 && selNum <= matches.Count)
    {
        var toDelete = matches[selNum - 1];
        Console.Write($"Delete {toDelete}? (y/n): ");
        var conf = Console.ReadLine();
        if (conf != null && conf.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            products.RemoveAll(p => string.Equals(p, toDelete, StringComparison.OrdinalIgnoreCase));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Product deleted.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Deletion cancelled.");
            Console.ResetColor();
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid selection. Deletion cancelled.");
        Console.ResetColor();
    }
}

void ShowStatistics()
{
    Console.WriteLine();
    if (products.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("No products to analyze.");
        Console.ResetColor();
        return;
    }

    var numbers = products.Select(p =>
    {
        var index = p.IndexOf('-');
        return int.Parse(p.Substring(index + 1));
    }).ToList();

    var total = numbers.Count;
    var highest = numbers.Max();
    var lowest = numbers.Min();
    var average = numbers.Average();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Statistics:");
    Console.WriteLine($" Total products: {total}");
    Console.WriteLine($" Highest product number: {highest}");
    Console.WriteLine($" Lowest product number: {lowest}");
    Console.WriteLine($" Average product number: {average}");
    Console.ResetColor();
}

bool TryValidateAndNormalize(string input, out string normalized, out string error)
{
    normalized = string.Empty;
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(input))
    {
        error = "Input cannot be empty.";
        return false;
    }

    if (!input.Contains('-'))
    {
        error = "Product must contain a dash (-).";
        return false;
    }

    var parts = input.Split(new[] { '-' }, 2);
    var left = parts[0].Trim();
    var right = parts.Length > 1 ? parts[1].Trim() : string.Empty;

    // Validate left side
    if (string.IsNullOrEmpty(left) || !Regex.IsMatch(left, @"^[A-Za-z]+$"))
    {
        errors.Add("The left side must contain letters only.");
    }

    // Validate right side
    if (string.IsNullOrEmpty(right) || !Regex.IsMatch(right, @"^[0-9]+$"))
    {
        errors.Add("The right side must contain numbers only.");
    }
    else if (!int.TryParse(right, out var number))
    {
        errors.Add("The right side must contain numbers only.");
    }
    else if (number < 200 || number > 500)
    {
        errors.Add("The numeric part must be between 200-500.");
    }

    if (errors.Count > 0)
    {
        error = string.Join(" ", errors);
        return false;
    }

    normalized = $"{left.ToUpperInvariant()}-{int.Parse(right)}";
    error = string.Empty;
    return true;
}

/* --- Alternative validation implementation using regex ---

bool TryValidateAndNormalize(string input, out string normalized, out string error)
{
    normalized = string.Empty;
    error = string.Empty;

    var match = Regex.Match(input.Trim(), "^([A-Za-z]+)-([0-9]{1,3})$");
    if (!match.Success)
    {
        error = "Invalid format. Expected LETTERS-NUMBER (e.g. ABC-200).";
        return false;
    }

    var letters = match.Groups[1].Value;
    var numberStr = match.Groups[2].Value;
    var number = int.Parse(numberStr);

    if (number < 200 || number > 500)
    {
        error = "Number must be between 200 and 500 (inclusive).";
        return false;
    }

    // normalize: uppercase letters and numeric value
    normalized = $"{letters.ToUpperInvariant()}-{number}";
    return true;
}
*/