// See https://aka.ms/new-console-template for more information

using KH.Lab.RulesEnglineService;
using RulesEngine.Models;

var order = new Order()
{
    Id = 1,
    OrderDate = DateTime.Now,
    ServiceType = "B2C",
    ParentId = "733",
    Amount = 1020
};

//特定母代碼
var parentIds = new List<string>() { "733", "734", "735" };

//RulesEngine
var workflow = File.ReadAllText("OrderReturnAmount.json");

//註冊類別
var reSettings = new ReSettings { CustomTypes = new[] { typeof(Program) } };
var bre = new RulesEngine.RulesEngine(new[] { workflow }, reSettings);

//定義參數
var rp1 = new RuleParameter("order", order);
var rp2 = new RuleParameter("parentIds", parentIds);

var resultList = await bre.ExecuteAllRulesAsync("OrderWorkflow", rp1, rp2);
var errors = resultList.Where(c => !c.IsSuccess
&& (c.ActionResult.Output != null || c.ActionResult.Exception != null));
if (errors?.Any() == true)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"規則 {error.Rule.RuleName} 失敗， 錯誤訊息： {error.ExceptionMessage}");
    }
    return;
}

foreach (var result in resultList)
{
    Console.WriteLine($"規則 {result.Rule.RuleName} 執行成功，結果為： {result.ActionResult.Output}");
}

// 計算訂單金額
var returnAmount = Math.Round(order.Amount - resultList.Where(c => c.IsSuccess).Sum(c => decimal.Parse(c.ActionResult.Output.ToString())), 0, MidpointRounding.AwayFromZero);
Console.WriteLine($"訂單金額： {returnAmount} 元");