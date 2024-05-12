using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

class ServidorHttp
{

    //responsável por escutar qualquer porta a espera de qualquer tipo de conexão TCP.
    private TcpListener Controlador { get; set; }
    private int Porta {get; set; }
    private int QtdeRequests { get; set; }
    public string HtmlExemplo { get; set; }

    //tanto a chave (extensão do arquivo), quanto o valor (tipo mime em questão) serão do tipo string.
    private SortedList<string, string> TiposMime { get; set; }

    public ServidorHttp(int porta = 8080)
    {        
        Porta = porta;
        CriarHtmlExemplo();
        PopularTiposMime();
        try
        {
            Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), Porta);
            Controlador.Start();
            Console.WriteLine($"Servidor HTTP está rodando na porta {Porta}.");
            Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{Porta}");
            Task servidorHttpTask = Task.Run(() => AguardarRequests());
            servidorHttpTask.GetAwaiter().GetResult();
        }
        catch (Exception e)        
        {
            Console.WriteLine($"Erro ao iniciar servidor na porta {Porta}: \n{e.Message}");
        }        
    }    

    private async Task AguardarRequests()
    {
        while(true)
        {
            Socket conexao = await Controlador.AcceptSocketAsync();
            QtdeRequests++;
            Task task = Task.Run(() => ProcessarRequest(conexao, QtdeRequests));
        }
    }

    private void ProcessarRequest(Socket conexao, int numeroRequests)
    {
        Console.WriteLine($"Processando request #{numeroRequests}...\n");
        if(conexao.Connected)
        {
            byte[] bytesRequisicao = new byte[1024];
            conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);
            string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao)
                .Replace((char)0, ' ').Trim();
            if(textoRequisicao.Length > 0)
            {
                Console.WriteLine($"\n{textoRequisicao}\n");
                
                string[] linhas = textoRequisicao.Split("\r\n");                
                int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                int iSegundoEspaco = linhas[0].LastIndexOf(' ');
                string metodoHttp = linhas[0].Substring(0, iPrimeiroEspaco);               
                string recursoBuscado = linhas[0].Substring(iPrimeiroEspaco + 1, iSegundoEspaco - iPrimeiroEspaco - 1);
                if (recursoBuscado == "/") recursoBuscado = "/index.html";
                string textoParametros = recursoBuscado.Contains("?") ?
                    recursoBuscado.Split("?")[1]: "";
                SortedList<string, string> parametros = ProcessarParametros(textoParametros); 
                string dadosPost = textoRequisicao.Contains("\r\n\r\n") ?
                    textoRequisicao.Split("\r\n\r\n")[1] : "";

                Console.WriteLine($"Estes são os dados Texto Requisicao:{textoRequisicao}"); 
                Console.WriteLine("-------------------------------------------------"); 
                string[] palavras = textoRequisicao.Split(' ');
                foreach (string caractereAtual in palavras)
                {
                    Console.WriteLine($"Caractere atual: '{caractereAtual}'");
                }

                Console.WriteLine("-------------------------------------------------");    
                if (!string.IsNullOrEmpty(dadosPost))
                {
                    dadosPost = HttpUtility.UrlDecode(dadosPost, Encoding.UTF8);
                    var parametrosPost = ProcessarParametros(dadosPost);
                    foreach (var pp in parametrosPost)
                        parametros.Add(pp.Key, pp.Value);
                }      
                recursoBuscado = recursoBuscado.Split("?")[0];
                string versaoHttp = linhas[0].Substring(iSegundoEspaco + 1);
                iPrimeiroEspaco = linhas[1].IndexOf(' ');
                string nomeHost = linhas[1].Substring(iPrimeiroEspaco + 1);        

                //var bytesConteudo = Encoding.UTF8.GetBytes(HtmlExemplo, 0, HtmlExemplo.Length);
                //var bytesConteudo = LerArquivo("/index.html");
                //var bytesConteudo = LerArquivo(recursoBuscado);
                byte[] bytesConteudo = null;
                byte[] bytesCabecalho = null;
                FileInfo fiArquivo = new FileInfo(ObterCaminhoFisicoArquivo(recursoBuscado));
                if (fiArquivo.Exists)
                {
                    /* if(bytesConteudo.Length > 0)
                    {
                        bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8", "200", bytesConteudo.Length); 
                    }   */ 
                    if (TiposMime.ContainsKey(fiArquivo.Extension.ToLower()))
                    {
                        //bytesConteudo = File.ReadAllBytes(fiArquivo.FullName);
                        if (fiArquivo.Extension.ToLower() == ".dhtml")
                            bytesConteudo = GerarHtmlDinamico(fiArquivo.FullName, parametros, metodoHttp);
                        else
                            bytesConteudo = File.ReadAllBytes(fiArquivo.FullName); 

                        
                        string tipoMime = TiposMime[fiArquivo.Extension.ToLower()];
                        bytesCabecalho = GerarCabecalho(versaoHttp, tipoMime, "200", bytesConteudo.Length); 
                    }
                    else
                    {
                        bytesConteudo = Encoding.UTF8.GetBytes(
                            "<h1>Erro 415 - Tipo de arquivo não suportado.</h1>");
                        bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8", 
                            "415", bytesConteudo.Length);  
                        
                    }                 
                }
                else
                {
                    bytesConteudo = Encoding.UTF8.GetBytes("<h1>Erro 404 - Arquivo não encontrado</h1>");
                    bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8", "200", bytesConteudo.Length);
                }    
                Console.WriteLine($"Este é o recurso buscado - {recursoBuscado}");
                //var bytesCabecalho = GerarCabecalho("Http/ 1.1", "text/html;charset-utf-8", "200", bytesConteudo.Length); 
                               
                int bytesEnviados = conexao.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                bytesEnviados += conexao.Send(bytesConteudo, bytesConteudo.Length, 0);
                conexao.Close();
                Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta à requisição #{numeroRequests}.");
            }    
        }
        Console.WriteLine($"\nRequest {numeroRequests} finalizado. ");
    }

    public byte[] GerarCabecalho(string versaoHttp, string tipoMime, string codigoHttp, int qtdeBytes = 0)
    {
        StringBuilder texto = new StringBuilder();
        texto.Append($"{versaoHttp} {codigoHttp}{Environment.NewLine}");
        texto.Append($"Server: Servidor Http Simples 1.0{Environment.NewLine}");
        texto.Append($"Content-Type: {tipoMime}{Environment.NewLine}");
        texto.Append($"Content-Length: {qtdeBytes}{Environment.NewLine}{Environment.NewLine}");
        return Encoding.UTF8.GetBytes(texto.ToString());
    }

    private void CriarHtmlExemplo()
    {
        StringBuilder html = new StringBuilder();
        html.Append("<!DOCTYPE html><html lang=\"pt-br\"><head><meta charset=\"UTF8\">");
        html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.Append("<title>Página Estática</title><head><body>");
        html.Append("<h1>Página Estática</h1><body></html>");
        this.HtmlExemplo = html.ToString();
    }

    /* public byte[] LerArquivo(string recurso)
    {
        string diretorio = "C:\\Studies\\Languages\\Csharp\\Aspnet\\ServidorHttpSimples\\www";
        string caminhoArquivo = diretorio + recurso.Replace("/", "\\");
        Console.WriteLine(caminhoArquivo);
        if(File.Exists(caminhoArquivo))
        {
            Console.WriteLine($"O caminho do Arquivo é {caminhoArquivo}");
            return File.ReadAllBytes(caminhoArquivo);            
        }
        else return new byte[0];
    } */

    private void PopularTiposMime()
    {
        TiposMime = new SortedList<string, string>();
        //Adicionando suporte aos tipos mime:
        TiposMime.Add(".html", "text/html;charset=utf-8");
        TiposMime.Add(".htm", "text/html;charset=utf-8");
        TiposMime.Add(".css", "text/css");
        TiposMime.Add(".js", "text/javascript");
        TiposMime.Add(".png", "image/png");
        TiposMime.Add(".jpg", "image/jpeg");
        TiposMime.Add(".gif", "image/gif");
        TiposMime.Add(".svg", "image/svg+xml");
        TiposMime.Add(".webp", "image/webp");
        TiposMime.Add(".ico", "image/ico");
        TiposMime.Add(".woff", "font/woff");
        TiposMime.Add(".woff2", "font/woff2");
        TiposMime.Add(".dhtml", "text/html;charset=utf-8");
    }

    public string ObterCaminhoFisicoArquivo(string arquivo)
    {
        string caminhoArquivo = "C:\\Studies\\Languages\\Csharp\\Aspnet\\ServidorHttpSimples\\www" + arquivo.Replace("/", "\\");
        return caminhoArquivo;
    }

    //public byte[] GerarHTMLDinamico(string caminhoArquivo, SortedList<string, string> parametros)
    private Byte[] GerarHtmlDinamico(string caminhoArquivo, SortedList<string, string> parametros, string metodoHttp)
    {
        /* string coringa = "{{HtmlGerado}}";
        string htmlModelo = File.ReadAllText(caminhoArquivo);
        StringBuilder htmlGerado = new StringBuilder(); */

        //primeiro comentário
        /* htmlGerado.Append("<ul>");
        foreach (var tipo in TiposMime.Keys)
        {
            htmlGerado.Append($"<li>Arquivos com extensão {tipo}</li>");
        } */

        //Segundo Comentário
       /*  htmlGerado.Append("</ul>");
        if(parametros.Count > 0)
        {
            foreach (var p in parametros)
            {
                htmlGerado.Append($"<li>Arquivos com extensão {p.Key}={p.Value}</li>");
            }
            htmlGerado.Append("</ul>");
        }
        else
        {
            htmlGerado.Append($"<p>Nenhum parâmetro foi passado</p>");
        }    
            string textoHtmlGerado = htmlModelo.Replace(coringa, htmlGerado.ToString()); */


        //return Encoding.UTF8.GetBytes(textoHtmlGerado, 0, textoHtmlGerado.Length);
        /* SortedList<string, string> parametros = new SortedList<string, string>();
        if (!string.IsNullOrEmpty(textoParametros.Trim())
        {
            string[] paresChaveValor = textoParametros.Trim());
        }
        return parametros; */
        FileInfo fiArquivo = new FileInfo(caminhoArquivo);
        string nomeClassePagina = "Pagina" + fiArquivo.Name.Replace(fiArquivo.Extension, "");
        
        Type? tipoPaginaDinamica = Type.GetType(nomeClassePagina, true, true);
        PaginaDinamica? pd = Activator.CreateInstance(tipoPaginaDinamica) as PaginaDinamica;
        pd.HtmlModelo = File.ReadAllText(caminhoArquivo);
        switch (metodoHttp.ToLower())
        {
            case "get":
                return pd.Get(parametros);
            case "post":
                return pd.Post(parametros);
            default:
                return new byte[0];     
        }
    }
    public SortedList<string, string> ProcessarParametros(string textoParametros)
    {
        SortedList<string, string> parametros = new SortedList<string, string>();
        //O & separa os pares chave/ valor e o que separa a chave de um valor é o símbolo de igualdade.
        if (!string.IsNullOrEmpty(textoParametros.Trim()))
        {
            string[] paresChaveValor = textoParametros.Split("&");
            foreach(var par in paresChaveValor)
            {
                parametros.Add(par.Split("=")[0].ToLower(), par.Split("=")[1].ToLower());
            }
        }

        return parametros; 
    }
}