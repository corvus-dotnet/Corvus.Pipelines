namespace InvoiceExample;

public readonly record struct Invoice(InvoiceItem[] LineItems);

public readonly record struct InvoiceItem(string Description, decimal Price, int Quantity);