using CouchDB.Client;
using CouchDB.Client.FluentMango;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CouchDB.Dados
{
    /// <summary>
    /// CouchDBDados
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class CouchDBDados<TType> where TType : class
    {
        string connectionString, dataBase;
        /// <summary>
        /// construtor padrão
        /// </summary>
        public CouchDBDados()
        {
        }

        /// <summary>
        /// construtor com dados de conexão.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <param name="database"></param>
        public CouchDBDados(string connectionstring, string database)
        {
            this.connectionString = connectionstring;
            this.dataBase = database;
        }

        /// <summary>
        /// obter todos os registros de tipo TType no banco de dados
        /// </summary>
        /// <param name="camposSelect"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetAsync(string[] camposSelect)
        {
            //verifica parâmetros que são de preenchimento obrigatório.
            if (camposSelect == null || camposSelect.Length == 0)
            {
                throw new Exception("Parâmetros inválidos!");
            }

            //conectar no banco de dados.
            CouchClient couchClient = new CouchClient(connectionString);
            CouchDatabase couchDataBase = await couchClient.GetDatabaseAsync(dataBase);

            //É preciso informar rquais campos serão retornados na consulta ao CouchDB
            FindBuilder findBuilder = new FindBuilder().Fields(camposSelect);

            //obtem registros em banco de dados.
            CouchResponse couchResponse = await couchDataBase.SelectAsync(findBuilder);

            //valida o status do retorno a consulta ao CouchDB
            if (couchResponse.StatusCode == HttpStatusCode.OK)
            {
                //transforma o resultado em JsonTextReader
                JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(couchResponse.Docs.ToString()));

                //transforma o reader em objeto do tipo TType.
                TType[] result = new JsonSerializer().Deserialize(jsonTextReader, typeof(TType[])) as TType[];

                return new JsonResult(result);
            }
            else
            {
                //retorna o erro ocorrido durante a pesquisa do CouchDB
                return new JsonResult(couchResponse.Content);
            }
        }

        public async Task<JsonResult> GetAsync(string _id, string[] camposSelect)
        {
            try
            {
                //verifica parâmetros que são de preenchimento obrigatório.
                if (camposSelect == null || camposSelect.Length == 0 || string.IsNullOrEmpty(_id))
                {
                    throw new Exception("Parâmetros inválidos!");
                }

                //conectar no banco de dados.
                CouchClient couchClient = new CouchClient(connectionString);
                CouchDatabase couchDataBase = await couchClient.GetDatabaseAsync(dataBase);

                /*Cria condição (FindBuilder.Selector) que será adicionada ao select*/
                FindBuilder findBuilder = (new FindBuilder()).Selector("_id", SelectorOperator.Equals, _id);

                //obtem registros no CouchDB
                CouchResponse couchResponse = await couchDataBase.SelectAsync(findBuilder);
                
                //valida o status do retorno a consulta ao CouchDB
                if (couchResponse.StatusCode == HttpStatusCode.OK)
                {
                    //transforma o resultado em JsonTextReader
                    JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(couchResponse.Docs[0].ToString()));

                    //transforma o reader em objeto do tipo TType.
                    TType result = (TType)new JsonSerializer().Deserialize(jsonTextReader, typeof(TType));

                    return new JsonResult(result);
                }
                else
                {
                    //retorna o erro ocorrido durante a pesquisa do CouchDB
                    return new JsonResult(couchResponse.Content);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        public async Task<JsonResult> PostAsync(TType type)
        {
            try
            {
                //verifica parâmetros que são de preenchimento obrigatório.
                if (type == null)
                {
                    throw new Exception("Parâmetros inválidos!");
                }

                //conectar no banco de dados.
                CouchClient couchClient = new CouchClient(connectionString);
                CouchDatabase couchDataBase = await couchClient.GetDatabaseAsync(dataBase);

                //Os campos _id e _rev são gerados automaticamente pelo CouchDB. Quando fazemos uma inserção, devemos remover estes campos do objeto a ser persistido. 
                //Vale lembrar que estes mesmos dois campos são obrigatórios para alteração/exclusão de documentos.
                JObject jObject = JObject.FromObject(type);
                jObject.Remove("_id");
                jObject.Remove("_rev");

                //persiste o novo documento
                CouchResponse couchResponse = await couchDataBase.InsertAsync(jObject);
                
                //valida o status do retorno a consulta ao CouchDB
                if (couchResponse.StatusCode == HttpStatusCode.Created ||
                    couchResponse.StatusCode == HttpStatusCode.Accepted ||
                    couchResponse.StatusCode == HttpStatusCode.OK)
                {
                    object chaveRetorno = new { _id = couchResponse.Id, _rev = couchResponse.Rev };

                    return new JsonResult(chaveRetorno);
                }
                else
                {
                    //retorna o erro ocorrido durante a pesquisa do CouchDB
                    return new JsonResult(couchResponse.Content);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        public async Task<JsonResult> PutAsync(TType type)
        {
            try
            {
                //verifica parâmetros que são de preenchimento obrigatório.
                if (type == null)
                {
                    throw new Exception("Parâmetros inválidos!");
                }

                //conectar no banco de dados.
                CouchClient couchClient = new CouchClient(connectionString);
                CouchDatabase couchDataBase = await couchClient.GetDatabaseAsync(dataBase);

                //prepara objeto type para ser persistido.
                JObject jObject = JObject.FromObject(type);

                //atualiza o documento no CouchDB
                CouchResponse couchResponse = await couchDataBase.UpdateAsync(JToken.Parse(jObject.ToString()));

                if (couchResponse.StatusCode == HttpStatusCode.Created ||
                    couchResponse.StatusCode == HttpStatusCode.Accepted ||
                    couchResponse.StatusCode == HttpStatusCode.OK)
                {
                    return new JsonResult(type);
                }
                else
                {
                    //retorna o erro ocorrido durante a pesquisa do CouchDB
                    return new JsonResult(couchResponse.Content);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
        public async Task<JsonResult> DeleteAsync(string id, string rev)
        {
            try
            {
                //se não houve campos para serem listados, deverá retornar a lista vazia.
                if (Guid.Equals(id, Guid.Empty))
                {
                    throw new Exception("Parâmetros inválidos!");
                }
                
                //conectar no banco de dados.
                CouchClient couchClient = new CouchClient(connectionString);
                CouchDatabase couchDataBase = await couchClient.GetDatabaseAsync(dataBase);

                //remove o documento.
                CouchResponse couchResponse = await couchDataBase.DeleteAsync(id, rev);

                //verifica se houve a exclusão
                if (couchResponse.StatusCode == HttpStatusCode.Accepted ||
                    couchResponse.StatusCode == HttpStatusCode.OK)
                {
                    return new JsonResult("Ok");
                }
                else
                {
                    //retorna o erro ocorrido durante a pesquisa do CouchDB
                    return new JsonResult(couchResponse.Content);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

    }
}