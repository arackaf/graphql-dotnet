using HotChocolate;
using HotChocolate.Execution;
using Newtonsoft.Json;
using System;
using System.Collections;

namespace graphql_sandbox
{


    public class Comment
    {
        public string text { get; set; }
    }

    public class Task
    {
        public string title { get; set; }
        public string description { get; set; }

        public System.Threading.Tasks.Task<Comment[]> Comments()
        {
            Console.WriteLine("Comments Called");   
            return System.Threading.Tasks.Task<Comment[]>.Run(() => new Comment[] { new Comment() { text = "Hello" } });
                //k new Comment[] { };
        }
    }

    public class TaskResolvers
    {
        public Task[] AllTasks(string title, bool hasAttachments, bool hasComments)
        {
            Console.WriteLine("title {0} {1} {2}", title, hasAttachments, hasComments);

            return new Task[] { new Task { title = "t1", description = "d1" }, new Task { title = "t2", description = "d2" } };
        }
    }

    class Program
    {
        static string GQLSchema = @"
            type Comment { 
                text: String
            }
            type Task {
                title: String
                description: String
                comments: [Comment]
            }

            type Query {
                allTasks(title: String, hasAttachments: Boolean, hasComments: Boolean): [Task]
            }
        ";

        static void Main(string[] args)
        {
            try
            {
                var schema = Schema.Create(GQLSchema,
                    c =>
                    {
                        c.BindType<Comment>().To("Comment");
                        c.BindType<Task>().To("Task");
                        c.BindType<TaskResolvers>().To("Query"); //(() => "world").To("Query", "hello"));
                    });

                for (; ; )
                {
                    Console.WriteLine("Enter Query");
                    string query = Console.ReadLine();
                    var t = schema.ExecuteAsync(query);
                    t.Wait();
                    var queryResult = t.Result;
                    if (queryResult?.Errors?.Count > 0)
                    {
                        foreach(var e in queryResult.Errors)
                        {
                            Console.WriteLine(e.Message.ToString());
                        }
                    }
                    else
                    {
                        foreach (string k in queryResult.Data.Keys)
                        {
                            var list = ((IList)queryResult.Data[k]);
                            foreach(var o in list)
                            {
                                if (o is OrderedDictionary)
                                {
                                    display(o as OrderedDictionary);
                                } 
                            }
                        }
                    }
                }

            }
            catch(SchemaException e)
            {
                var errors = e.Errors.GetEnumerator();
                while (errors.MoveNext())
                {
                    SchemaError error = errors.Current;
                    Console.WriteLine(error.Message);
                    Console.WriteLine(error.ToString());
                }
            }            
        }

        private static void display(OrderedDictionary v)
        {
            Console.WriteLine(JsonConvert.SerializeObject(v));
        }
    }
}
