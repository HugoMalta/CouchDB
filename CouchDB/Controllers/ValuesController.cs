using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Dados;
using CouchDB.Model;
using Microsoft.AspNetCore.Mvc;

namespace CouchDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly string ConnectionString = "http://admin:admin@localhost:5984";
        private readonly string DataBaseName = "pessoa";

        /// <summary>
        /// Obter lista de documentos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Get()
        {
            CouchDBDados<Pessoa> a = new CouchDBDados<Pessoa>(ConnectionString, DataBaseName);
            Task<JsonResult> tese = a.GetAsync(new string[] { "_id", "_rev", "Nome", "CPF", "Idade" });
            Task.WaitAll(tese);
            return new JsonResult(tese.Result);
            //return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Obter um documento.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public JsonResult Get(string id)
        {
            CouchDBDados<Pessoa> a = new CouchDBDados<Pessoa>(ConnectionString, DataBaseName);
            Task<JsonResult> tese = a.GetAsync(id, new string[] { "_id", "_rev", "Nome", "CPF", "Idade" });
            Task.WaitAll(tese);
            return new JsonResult(tese.Result);

        }

        /// <summary>
        /// Inserir documento
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Post([FromBody] Pessoa value)
        {
            CouchDBDados<Pessoa> a = new CouchDBDados<Pessoa>(ConnectionString, DataBaseName);
            Task<JsonResult> tese = a.PostAsync(value);
            Task.WaitAll(tese);
            return new JsonResult(tese.Result);
        }

        /// <summary>
        /// Alterar documento
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        public JsonResult Put([FromBody] Pessoa value)
        {
            CouchDBDados<Pessoa> a = new CouchDBDados<Pessoa>(ConnectionString, DataBaseName);
            Task<JsonResult> tese = a.PutAsync(value);
            Task.WaitAll(tese);
            return new JsonResult(tese.Result);
        }

        /// <summary>
        /// Remover documentos
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_rev"></param>
        /// <returns></returns>
        [HttpDelete]
        public JsonResult Delete(string _id, string _rev)
        {
            CouchDBDados<Pessoa> a = new CouchDBDados<Pessoa>(ConnectionString, DataBaseName);
            Task<JsonResult> tese = a.DeleteAsync(_id, _rev);
            Task.WaitAll(tese);
            return new JsonResult(tese.Result);
        }
    }
}
