using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ToDoList.Models.Filters;

public class ApiResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var success = IsSuccessStatusCode(objectResult.StatusCode);
            var message = success ? "Request successful" : "Request failed";

            // 將結果轉換為 ApiResponse
            var apiResponse = new ApiResponse(
                success: success,
                message: message,
                data: success ? objectResult.Value : null,
                errors: success ? null : objectResult.Value // 將錯誤內容放入 error
            );

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = objectResult.StatusCode
            };
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            var success = IsSuccessStatusCode(statusCodeResult.StatusCode);
            var message = success ? "Request successful" : "Request failed";

            var apiResponse = new ApiResponse(
                success: success,
                message: message,
                data: null,
                errors: success ? null : new { statusCodeResult.StatusCode } // 錯誤時提供簡單資訊
            );

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = statusCodeResult.StatusCode
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // 不做任何操作
    }

    /// <summary>
    /// 判斷是否為成功的狀態碼
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    private bool IsSuccessStatusCode(int? statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }
}