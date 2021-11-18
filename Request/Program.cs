using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Request
{
    internal class Program
    {
        static HttpClient client = new HttpClient();

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class Todo
        {
            [JsonProperty(PropertyName = "Id")]
            public int Id { get; set; }
            [JsonProperty(PropertyName = "Name")]
            public string Name { get; set; }
            [JsonProperty(PropertyName = "IsComplete")]
            public bool IsComplete { get; set; }
        }

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        static async Task<Todo> GetProductAsync(string path)
        {
            Todo todo = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                todo = await response.Content.ReadAsAsync<Todo>();

            }
            return todo;
        }

        static async Task<int> GetCount()
        {
            Todo todo = null;
            var orgUnitParentGrade = 0;
            HttpResponseMessage response = await client.GetAsync("/count");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                orgUnitParentGrade = JsonConvert.DeserializeObject<int>(data);

            }
            return orgUnitParentGrade;
        }

        static async Task<Uri> CreateProductAsync(Todo todo)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "/todoitems", todo);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static void ShowProduct(Todo todo)
        {
            Console.WriteLine($"Id: {todo.Id}\tName: " +
                $"{todo.Name}\tIsComplete: {todo.IsComplete}");
        }

        static async Task<HttpStatusCode> DeleteProductAsync(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync(
                $"/todoitems/{id}");
            return response.StatusCode;
        }

        static async Task<Todo> UpdateProductAsync(Todo todo)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"/todoitems/{todo.Id}", todo);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            todo = await response.Content.ReadAsAsync<Todo>();
            return todo;
        }


        static async Task RunAsync()
        {
            // Update port # in the following line.
            client.BaseAddress = new Uri("https://localhost:7258/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                for (int i = 1; i <= 6; i++)
                {
                    // Create a new todo
                    Todo todo = new Todo();
                    todo.Id = i;
                    todo.Name = "Alex";
                    todo.IsComplete = false;

                    var url = await CreateProductAsync(todo);
                    Console.WriteLine($"Created at {url}");

                    todo = await GetProductAsync(url.OriginalString);
                    ShowProduct(todo);

                    var count = await GetCount();
                    Console.WriteLine($"Количество записей: {count}");

                    if (count >= 6)
                    {
                        var statusCode = await DeleteProductAsync(1);
                        Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");
                        Console.WriteLine("Первая запись удалена...");
                    }

                    Console.WriteLine();
                }
                
                /*
                // Update the Name
                Console.WriteLine("Updating price...");
                todo.Name = "Kat";
                await UpdateProductAsync(todo);
                */
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();
        }
    }
}