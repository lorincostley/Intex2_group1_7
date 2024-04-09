namespace Intex2.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        public virtual void AddItem(Product proj, int quantity)
        {
            CartLine? line = Lines
                .Where(x => x.Product.ProductId == proj.ProductId)
                .FirstOrDefault();
            if (line == null)
            {
                Lines.Add(new CartLine
                {
                    Product = proj,
                    Quantity = quantity
                });
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        public virtual void RemoveLine(Product proj) => Lines.RemoveAll(x => x.Product.ProductId == proj.ProductId);

        public virtual void Clear() => Lines.Clear();

        public decimal CalculateTotal()
        {
            var blah = Lines.Sum(x => 25 * x.Quantity);
            return (blah);
        }
        public class CartLine
        {
            public int CartLineId { get; set; }
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }
    }
}
