// See https://aka.ms/new-console-template for more information


using KH.Lab.RulesEngineLab;
using RulesEngine.Models;

var player = new Player
{
    Id = 1,
    GameType = 1,
    PlaceBet = 201
};
//特定遊戲代碼
var gameTypes = new[] { 1, 3, 8 };

//RulesEngine
var workflow = System.IO.File.ReadAllText("PlayerReturnBonus.json");
//註冊類別
var reSettings = new ReSettings { CustomTypes = new[] { typeof(Program) } };
var bre = new RulesEngine.RulesEngine(new[] { workflow }, reSettings);
//定義參數
var rp1 = new RuleParameter("player", player);
var rp2 = new RuleParameter("gameTypes", gameTypes);

var resultList = await bre.ExecuteAllRulesAsync("PlayerBonusWorkflow", rp1, rp2);
var errors = resultList.Where(c => !c.IsSuccess
&& (c.ActionResult.Output != null || c.ActionResult.Exception != null));
if (errors?.Any() == true)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Rule {error.Rule.RuleName} failed with error: {error.ExceptionMessage}");
    }
    return;
}

foreach (var result in resultList)
{
    Console.WriteLine($"Rule {result.Rule.RuleName} executed with result: {result.ActionResult.Output}");
}

var returnBonus = resultList.Where(c => c.IsSuccess)
   .Sum(c => decimal.Parse(c.ActionResult.Output.ToString()));
Console.WriteLine($"Return Bonus: {returnBonus}");