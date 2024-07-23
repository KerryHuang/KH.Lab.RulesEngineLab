using RulesEngine.Models;

namespace KH.Lab.RulesEnglineService
{
    internal static class WorkflowDefine
    {
        public static Workflow GetWorkflow()
        {
            // 定義規則
            var workflow = new Workflow
            {
                WorkflowName = "OrderWorkflow",
                Rules =
                [
                    new Rule
                    {
                        RuleName = "基本折扣",
                        SuccessEvent = "計算基本折扣",
                        ErrorMessage = "計算基本折扣錯誤",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "order.Amount > 0",
                        Actions = new RuleActions
                        {
                            OnSuccess = new ActionInfo
                            {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "Expression", "order.Amount * 0.05m" } }
                            }
                        }
                    },
                    new Rule
                    {
                        RuleName = "特定母代號",
                        SuccessEvent = "針對特定母代號給額外的折扣",
                        ErrorMessage = "針對特定母代號給額外的折扣錯誤",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "parentIds.Contains(order.ParentId) && order.Amount > 100 && order.Amount < 1000",
                        Actions = new RuleActions
                        {
                            OnSuccess = new ActionInfo
                            {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "Expression", "order.Amount * 0.01m" } }
                            }
                        }
                    },
                    new Rule
                    {
                        RuleName = "特定母代號中消費",
                        SuccessEvent = "針對特定母代號且中消費給額外的折扣",
                        ErrorMessage = "針對特定母代號且中消費給額外的折扣錯誤",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "parentIds.Contains(order.ParentId) && order.Amount > 1000 && order.Amount < 2000",
                        Actions = new RuleActions
                        {
                            OnSuccess = new ActionInfo
                            {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "Expression", "order.Amount  * 0.02m" } }
                            }
                        }
                    },
                    new Rule
                    {
                        RuleName = "特定母代號高消費",
                        SuccessEvent = "針對特定母代號且高消費給額外的折扣",
                        ErrorMessage = "針對特定母代號且高消費給額外的折扣錯誤",
                        RuleExpressionType = RuleExpressionType.LambdaExpression,
                        Expression = "parentIds.Contains(order.ParentId) && order.Amount > 3000",
                        Actions = new RuleActions
                        {
                            OnSuccess = new ActionInfo
                            {
                                Name = "OutputExpression",
                                Context = new Dictionary<string, object> { { "Expression", "order.Amount  * 0.03m" } }
                            }
                        }
                    }
                ]
            };

            return workflow;
        }
    }
}
