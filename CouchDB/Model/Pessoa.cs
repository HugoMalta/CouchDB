using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouchDB.Model
{
    public class Pessoa
    {
        public string _id { get; set; } = Guid.NewGuid().ToString("N");
        public string _rev { get; set; } = "1-"+ Guid.NewGuid().ToString("N");
        public string Nome { get; set; }
        public string CPF { get; set; }
        public int Idade { get; set; }

        public Pessoa() {
        }
        
        public Pessoa(string _id, string _rev, string nome, string cpf, string idade)
        {
            this._id = _id;
            this._rev = _rev;
            this.Nome = nome;
            this.CPF = cpf;
            try
            {
                int novaIdade;
                int.TryParse(idade, out novaIdade);
                this.Idade = novaIdade;
            } catch (Exception) { }
        }
        public Pessoa(string _id, string _rev, string nome, string cpf, int idade)
        {
            this._id = _id;
            this._rev = _rev;
            this.Nome = nome;
            this.CPF = cpf;
            this.Idade = idade;
        }
    }
}
