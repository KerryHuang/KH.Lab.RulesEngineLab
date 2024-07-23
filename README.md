# 試玩 RulesEngine

微軟使用C#開發一套RulesEngine，將規則進行高度抽象，期望核心商業邏輯穩定，而規則的擴充或修改可透過一種動態又簡單方式來進行，進而避免修改程式碼，而且還無須定義模型類別，更貼近真實世界動態輸入行為，最終輸出預期結果。

[官方說明 RulesEngine](https://microsoft.github.io/RulesEngine/?source=post_page-----cdb7731b6023--------------------------------)

特性:

- Json based rules definition ( Json定義規則 )
- Multiple input support (支援多輸入)
- Dynamic object input support (支援動態物件輸入)
- C# Expression support (支援C# 表達式)
- Extending expression via custom class/type injection (支援自訂義類別擴充表達式並注入)
- Scoped parameters (範圍参数)

情境

電子商務訂單依照訂單金額與特定客戶給予折扣

折扣規則：

1. 每筆訂單基本折扣為5%
2. 一般消費客群的特定客戶且訂單金額大於100元且小於1000元折扣為1%
3. 中消費客群的特定客戶且訂單金額大於1000元且小於2000元折扣為2%
4. 高消費客群的特定客戶且訂單金額大於300元折扣為3%

以上計算出折扣金額，此訂單金額減折扣金額等於訂單總金額。

結果，若之後規則增加或異動，只需要調整設定檔即可。



## 程式碼

1. JSON 設定檔

```json
{
  "WorkflowName": "OrderWorkflow",
  "Rules": [
    {
      "RuleName": "基本折扣",
      "SuccessEvent": "計算基本折扣",
      "ErrorMessage": "計算基本折扣錯誤",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "order.Amount > 0", //符合C#
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Expression": "order.Amount * 0.05m" //符合C#
          }
        }
      }
    },
    {
      "RuleName": "特定母代號",
      "SuccessEvent": "針對特定母代號給額外的折扣",
      "ErrorMessage": "針對特定母代號給額外的折扣錯誤",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "parentIds.Contains(order.ParentId) && order.Amount > 100 && order.Amount < 1000",
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Expression": "order.Amount * 0.01m"
          }
        }
      }
    },
    {
      "RuleName": "特定母代號中消費",
      "SuccessEvent": "針對特定母代號且中消費給額外的折扣",
      "ErrorMessage": "針對特定母代號且中消費給額外的折扣錯誤",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "parentIds.Contains(order.ParentId) && order.Amount > 1000 && order.Amount < 2000",
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Expression": "order.Amount  * 0.02m"
          }
        }
      }
    },
    {
      "RuleName": "特定母代號高消費",
      "SuccessEvent": "針對特定母代號且高消費給額外的折扣",
      "ErrorMessage": "針對特定母代號且高消費給額外的折扣錯誤",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "parentIds.Contains(order.ParentId) && order.Amount > 3000",
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Expression": "order.Amount  * 0.03m"
          }
        }
      }
    },
    {
      "RuleName": "週年慶消費",
      "SuccessEvent": "針對特定母代號且高消費給額外的折扣",
      "ErrorMessage": "針對特定母代號且高消費給額外的折扣錯誤",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "order.Amount > 1000",
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Expression": "order.Amount  * 0.5m"
          }
        }
      }
    }
  ]
}
```

C# 程式碼

```c#
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
```

