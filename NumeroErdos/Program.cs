using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NumeroErdos
{
    class Program
    {
        //private const string ENTRADA_PARA_TESTES_1 = "5\nP. Erdos, A. Selberg.\nP. Erdos, J. Silva, M. Souza.\nM. Souza, A. Selberg, A. Oliveira.\nJ. Ninguem, M. Ninguem.\nP. Duarte, A. Oliveira.\n2\nZ. Silva, P. Erdos.\nZ. Souza.\n0";
        //private const string ENTRADA_PARA_TESTES_2 = "5\nP. Erdos, A. Selberg.\nP. Erdos, J. Silva, M. Souza.\nM. Souza, A. Selberg, A. Oliveira.\nJ. Ninguem, M. Ninguem.\nP. Duarte, A. Oliveira.\n2\nZ. Silva, P. Erdos.\nZ. Souza.\n2\nZ. Silva, P. Lucas.\nZ. Souza.\n0";

        private const string NOME_ERDOS = "P. Erdos";
        private const string ERDOS_INFINITO = "infinito";
        private const int QUANTIDADE_MAXIMA_ARTIGOS = 100;

        static void Main(string[] args)
        {
            var linhas = new List<string>();

            do
            {
                linhas.Add(Console.ReadLine());
            } while (linhas[linhas.Count - 1] != "0");

            // Para testar o algoritmo, basta descomentar a linha abaixo e comentar as linhas acima.
            //var linhas = ENTRADA_PARA_TESTES_1.Split('\n').ToList();

            Console.Write(
                ProcessarNumeroErdos(linhas));
        }

        private class ConjuntoTeste
        {
            public int Numero { get; set; }
            public GrafoListaAdjacencia Grafo { get; set; }
            public Dictionary<string, int> IndiceAutores { get; set; }
            public int QuantidadeArtigos { get; set; }
        }

        private static string ProcessarNumeroErdos(List<string> linhas)
        {
            var resposta = new StringBuilder();

            var indiceTeste = 0;
            var numeroTeste = 0;

            while (linhas[indiceTeste] != "0")
            {
                numeroTeste++;

                var conjuntoTeste = new ConjuntoTeste
                {
                    Numero = numeroTeste,
                    Grafo = new GrafoListaAdjacencia(QUANTIDADE_MAXIMA_ARTIGOS),
                    IndiceAutores = new Dictionary<string, int>(),
                    QuantidadeArtigos = int.Parse(linhas[indiceTeste])
                };

                var indiceInicial = indiceTeste + 1;
                var indiceFinal = (indiceInicial + conjuntoTeste.QuantidadeArtigos) - 1;

                for (int i = indiceInicial; i <= indiceFinal; i++)
                {
                    var linhaTratada = TratarLinhaAutores(linhas[i]);
                    var autoresArtigo = linhaTratada.Split(',');

                    AdicionarArestasAutores(ref conjuntoTeste, autoresArtigo);
                }

                indiceTeste = indiceFinal + 1;

                AdicionarConjuntoTestes(ref resposta, conjuntoTeste);
            }

            return resposta.ToString().Trim();
        }

        private static void AdicionarConjuntoTestes(ref StringBuilder resposta, ConjuntoTeste conjuntoTeste)
        {
            resposta.AppendLine($"Teste {conjuntoTeste.Numero}");
            List<string> autores = null;

            try
            {
                var inicial = conjuntoTeste.IndiceAutores[NOME_ERDOS];
                var buscaLargura = new BuscaLargura(conjuntoTeste.Grafo, inicial);

                autores = conjuntoTeste.IndiceAutores.Keys.ToList();
                autores.Remove(NOME_ERDOS);

                OrdernarAutores(ref autores);

                foreach (var autor in autores)
                {
                    string numeroErdos = RetornarNumeroErdos(buscaLargura, conjuntoTeste.IndiceAutores[autor]);
                    resposta.AppendLine($"{autor}: {numeroErdos}");
                }
            }
            catch (KeyNotFoundException)
            {
                autores = conjuntoTeste.IndiceAutores.Keys.ToList();
                OrdernarAutores(ref autores);

                foreach (var autor in autores)
                    resposta.AppendLine($"{autor}: {ERDOS_INFINITO}");
            }
            finally
            {
                resposta.AppendLine();
            }
        }

        private static void OrdernarAutores(ref List<string> autores)
        {
            autores.Sort();
            autores = autores.OrderBy(a => a.Split(' ')[1]).ToList();
        }

        private static string RetornarNumeroErdos(BuscaLargura buscaLargura, int indiceAutor)
        {
            int numeroErdos = buscaLargura.DistanciaPara[indiceAutor];
            return numeroErdos == -1 ? ERDOS_INFINITO : numeroErdos.ToString();
        }

        private static void AdicionarArestasAutores(ref ConjuntoTeste conjuntoTeste, string[] autoresArtigo)
        {
            IndexarAutor(ref conjuntoTeste, autoresArtigo[0]);

            for (int i = 0; i < autoresArtigo.Length - 1; i++)
            {
                for (int j = i + 1; j < autoresArtigo.Length; j++)
                {
                    IndexarAutor(ref conjuntoTeste, autoresArtigo[j]);

                    conjuntoTeste.Grafo.AdicionarAresta(
                        conjuntoTeste.IndiceAutores[autoresArtigo[i]],
                        conjuntoTeste.IndiceAutores[autoresArtigo[j]]);
                }

            }
        }

        private static void IndexarAutor(ref ConjuntoTeste conjuntoTeste, string autor)
        {
            if (!conjuntoTeste.IndiceAutores.Keys.Contains(autor))
                conjuntoTeste.IndiceAutores.Add(autor, conjuntoTeste.IndiceAutores.Count);
        }

        private static string TratarLinhaAutores(string linhaAutores)
        {
            return Regex.Replace(linhaAutores, ".$|(?<=,) ", string.Empty);
        }

        #region Implementação Grafo
        public class BuscaLargura
        {
            public int[] ArestaPara { get; }
            public int[] DistanciaPara { get; }
            public int Inicial { get; }

            public BuscaLargura(GrafoListaAdjacencia grafo, int inicial)
            {
                ArestaPara = new int[grafo.QuantidadeVertices];
                DistanciaPara = new int[grafo.QuantidadeVertices];

                for (int i = 0; i < grafo.QuantidadeVertices; i++)
                {
                    DistanciaPara[i] = -1;
                    ArestaPara[i] = -1;
                }

                Inicial = inicial;

                ExecutarBusca(grafo, inicial);
            }

            private void ExecutarBusca(GrafoListaAdjacencia grafo, int inicial)
            {
                var fila = new Queue<int>();
                fila.Enqueue(inicial);
                DistanciaPara[inicial] = 0;

                while (fila.Count != 0)
                {
                    int v = fila.Dequeue();

                    foreach (var w in grafo.Adjacencia[v])
                    {
                        if (DistanciaPara[w] == -1)
                        {
                            fila.Enqueue(w);
                            DistanciaPara[w] = DistanciaPara[v] + 1;
                            ArestaPara[w] = v;
                        }
                    }
                }
            }
        }

        public class GrafoListaAdjacencia
        {
            public int QuantidadeVertices { get; }

            public List<int>[] Adjacencia { get; }

            public GrafoListaAdjacencia(int quantidadeVertices)
            {
                QuantidadeVertices = quantidadeVertices;
                Adjacencia = new List<int>[QuantidadeVertices];

                for (int i = 0; i < QuantidadeVertices; i++)
                {
                    Adjacencia[i] = new List<int>();
                }
            }

            public void AdicionarAresta(int v, int w)
            {
                Adjacencia[v].Add(w);
                Adjacencia[w].Add(v);
            }
        }
        #endregion
    }
}