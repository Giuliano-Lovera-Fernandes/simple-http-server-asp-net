using System.Text;

/* abstract class PaginaDinamica
{
    public string? HtmlModelo { get; set;}
    public virtual byte[] Get(SortedList<string, string> parametros)
    {
        return Encoding.UTF8.GetBytes(HtmlModelo);
    }

    public virtual byte[] Post(SortedList<string, string> parametros)
    {
        return Encoding.UTF8.GetBytes(HtmlModelo);
    }
} */

abstract class PaginaDinamica
{
    public string? HtmlModelo { get; set; }

    public virtual byte[] Get(SortedList<string, string> parametros)
    {
        // Verifica se HtmlModelo é nulo antes de acessá-lo
        /* if (HtmlModelo != null)
        {
            return Encoding.UTF8.GetBytes(HtmlModelo);
        }
        else
        {
            // Lógica para lidar com HtmlModelo nulo, se necessário
            return new byte[0]; // Ou outra ação apropriada
        } */
        return Encoding.UTF8.GetBytes(HtmlModelo);
    }

    public virtual byte[] Post(SortedList<string, string> parametros)
    {
        // Verifica se HtmlModelo é nulo antes de acessá-lo
        /* if (HtmlModelo != null)
        {
            return Encoding.UTF8.GetBytes(HtmlModelo);
        }
        else
        {
            // Lógica para lidar com HtmlModelo nulo, se necessário
            return new byte[0]; // Ou outra ação apropriada
        } */
        return Encoding.UTF8.GetBytes(HtmlModelo);
    }
}
