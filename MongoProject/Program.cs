using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;


IMongoClient client;
IMongoDatabase db = null;
var collectionName = "Workers";

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

//������������
app.MapGet("/api/account/{login}/{password}", (string login, string password) =>
{
    client = new MongoClient("mongodb://" + login + ":" + password + "@db_db:27017/admin");
    db = client.GetDatabase("Project");
    db.GetCollection<BsonDocument>(collectionName).Find("{}").FirstOrDefault();
});

//�������� ������ ���� ����������
app.MapGet("/api/workers", () => {
    if (db != null) return db.GetCollection<Worker>(collectionName).Find("{}").ToListAsync();
    return null;
});

//�������� ����������, ��������������� �� ���������
app.MapGet("/api/workers/sortbypost", () =>
{
    var c = db.GetCollection<Worker>(collectionName).Aggregate().Sort("{Post : 1}").ToList();
    return c;
});

//�������� ����������, ��������������� �� ��������
app.MapGet("/api/workers/sortbysalary", () =>
    db.GetCollection<Worker>(collectionName).Aggregate().Sort("{Salary : 1}").ToList()
);

//�������� ���������� � ���������� ���������� � ����������� ����������
app.MapGet("/api/workers/countonpost", () => { 
    //�������� ������ � ��
    var c = db.GetCollection<Worker>(collectionName).Aggregate()
    .Group(
        new BsonDocument {
        { "_id", "$Post" },
        { "count", new BsonDocument("$sum", 1) },
    })
    .ToList();
    //�������� ������ � ������� ����
    List<CountWorkersOnPost> list = new List<CountWorkersOnPost>();
    foreach (var item in c)
    {
        var t = new CountWorkersOnPost
        {
            Id = item["_id"].ToString(),
            Count = item["count"].ToInt32()
        };
        list.Add(t);
    }
    return list;
});

//�������� ���������� � �����������, ������������ � ������� �������� ���������� �� ����������� ���������
app.MapGet("/api/workers/minmaxavgsalaryonpost", () =>
{
    //�������� ������ � ��
    var c = db.GetCollection<Worker>(collectionName).Aggregate()
    .Group(
        new BsonDocument {
            {"_id", "$Post" },
            { "avg", new BsonDocument("$avg", "$Salary") },
            { "min", new BsonDocument("$min", "$Salary") },
            { "max", new BsonDocument("$max", "$Salary") }
    }).ToList();
    //�������� ������ � ������� ����
    List<MinMaxAvgSalary> list = new List<MinMaxAvgSalary>();
    foreach (var item in c)
    {
        var t = new MinMaxAvgSalary
        {
            Id = item["_id"].ToString(),
            Avg = item["avg"].ToInt32(),
            Min = item["min"].ToInt32(),
            Max = item["max"].ToInt32(),
        };
        list.Add(t);
    }
    return list;
});

//�������� ���������� � ������� �������� ���������� �� ����������� ���������
app.MapGet("/api/workers/avgage", () =>
{
    //�������� ������ � ��
    var c = db.GetCollection<Worker>(collectionName).Aggregate()
    .Group(
        new BsonDocument {
            {"_id", "$Post" },
            { "avg", new BsonDocument("$avg", "$Age") },
    }).ToList();
    //�������� ������ � ������� ����
    List<AvgAge> list = new List<AvgAge>();
    foreach (var item in c)
    {
        var t = new AvgAge
        {
            Id = item["_id"].ToString(),
            Avg = item["avg"].ToInt32(),
        };
        list.Add(t);
    }
    return list;
});

//����� ��������� �� id
app.MapGet("/api/workers/{id}", async (string id) =>
{
    var worker = await db.GetCollection<Worker>(collectionName)
        .Find(p => p.Id == id)
        .FirstOrDefaultAsync();

    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (worker == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, ���������� ���
    return Results.Json(worker);
});
//�������� ���������
app.MapDelete("/api/workers/{id}", async (string id) =>
{
    var worker = await db.GetCollection<Worker>(collectionName).FindOneAndDeleteAsync(p => p.Id == id);
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (worker is null) return Results.NotFound(new { message = "������������ �� ������" });
    return Results.Json(worker);
});
// ��������� ��������� � ������
app.MapPost("/api/workers", async (Worker worker) => {

    
    await db.GetCollection<Worker>(collectionName).InsertOneAsync(worker);
    return worker;
});
// ���������� ������ � ���������
app.MapPut("/api/workers", async (Worker workerData) => {

    var worker = await db.GetCollection<Worker>(collectionName)
        .FindOneAndReplaceAsync(p => p.Id == workerData.Id, workerData, new() { ReturnDocument = ReturnDocument.After });
    if (worker == null)
        return Results.NotFound(new { message = "������������ �� ������" });
    return Results.Json(worker);
});

app.Run();

public class Worker
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public int Salary { get; set; }
    public string Post { get; set; } = "";
}
public class CountWorkersOnPost
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public int Count { get; set; }
}
public class MinMaxAvgSalary
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public int Min {  get; set; }
    public int Max { get; set; }
    public int Avg { get; set; }
}
public class AvgAge
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public int Avg { get; set; }
}