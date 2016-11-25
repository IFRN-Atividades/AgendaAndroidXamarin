using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Json;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace Agenda
{
    [Activity(Label = "Agenda", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : ListActivity
    {
        private JsonValue json;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            CarregarLista();

            Button btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            btnAdd.Click += delegate { Postar(); };

            Button btnDeletar = FindViewById<Button>(Resource.Id.btnDeletar);
            btnDeletar.Click += delegate { Deletar(); };

            Button btnAtualizar = FindViewById<Button>(Resource.Id.btnAtualizar);
            btnAtualizar.Click += delegate { Put(); };

        }

        public async void CarregarLista()
        {
            json = await Get();
            List<Models.Contato> lista = JsonConvert.DeserializeObject<List<Models.Contato>>(json);
            IList<IDictionary<string, object>> dados = new List<IDictionary<string, object>>();
            foreach (Models.Contato c in lista)
            {
                IDictionary<string, object> dado = new JavaDictionary<string, object>();
                dado.Add("Id", c.Id.ToString());
                dado.Add("Nome", c.Nome.ToString());
                dado.Add("Telefone", c.Telefone.ToString());
                dados.Add(dado);
            }

            string[] from = new String[] {"Id", "Nome", "Telefone" };
            int[] to = new int[] {Resource.Id.IdContato, Resource.Id.NomeContato, Resource.Id.TelefoneContato };
            int layout = Resource.Layout.ListaContatos;

            //EditText txtid = FindViewById<EditText>(Resource.Id.editTextNome);
            // EditText txtdesc = FindViewById<EditText>(Resource.Id.editTextTelefone);
            // ArrayList for data row
            // SimpleAdapter mapping static data to views in xml file
            SimpleAdapter adapter = new SimpleAdapter(this, dados, layout, from, to);

            ListView.Adapter = adapter;
        }

        private async Task<JsonValue> Get()
        {
            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://10.21.0.137/20131011110029/api/contato"));
            request.Method = "GET";

            // Send the request to the server and wait for the response:
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());
                    // Return the JSON document:
                    return jsonDoc.ToString();
                }
            }
        }

        public void Postar()
        {
            EditText txtNome = FindViewById<EditText>(Resource.Id.editTextNome);
            EditText txtTelefone = FindViewById<EditText>(Resource.Id.editTextTelefone);

            Models.Contato c = new Models.Contato
            {
                Nome = txtNome.Text,
                Telefone = txtTelefone.Text,
            };
            string s = JsonConvert.SerializeObject(c);
            Post(s);
        }

        private void Post(string s)
        {
            WebRequest request = WebRequest.Create("http://10.21.0.137/20131011110029/api/contato");

            request.Method = "POST";

            string postData = s;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/json";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
            CarregarLista();
        }

        private async void Delete()
        {
            EditText txtId = FindViewById<EditText>(Resource.Id.editId);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://10.21.0.137/20131011110029/");

                var id = txtId.Text;
                var cliente = await client.DeleteAsync(string.Format("api/contato/" + id));
            }
        }

        public void Deletar()
        {
            Delete();
            CarregarLista();
        }

        private async void Put()
        {
            EditText txtId = FindViewById<EditText>(Resource.Id.editId);
            EditText txtNome = FindViewById<EditText>(Resource.Id.editTextNome);
            EditText txtTelefone = FindViewById<EditText>(Resource.Id.editTextTelefone);

            Models.Contato x = new Models.Contato
            {
                Id = int.Parse(txtId.Text),
                Nome = txtNome.Text,
                Telefone = txtTelefone.Text,
            };

            string t =  JsonConvert.SerializeObject(x);
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://10.21.0.137/20131011110029");
            var content = new StringContent(t, Encoding.UTF8, "application/json");
            await httpClient.PutAsync("/api/contato/" + x.Id, content);
            CarregarLista();

        }
    }
}

