using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;

    public AiController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("Default")!;
        _httpClient = httpClientFactory.CreateClient();
    }

    [Authorize]
    [HttpPost("hr")]
    public async Task<IActionResult> AnalyzeHR([FromBody] AiRequest request)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        var url = "https://api.groq.com/openai/v1/chat/completions";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var sqlPayload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] {
                new { 
                    role = "system", 
                    content = @"Te egy NavetraERP HR Modul szakértő SQL generátor vagy. 
                    TÁBLÁK:
                    - Employees(first_name, last_name, birth_date, id_number, residence_number, health_insurance_number, tax_id_number, bank_account_number, address_id, temp_address_id, hire_date, department_id, position_id, user_id, email, phone_number, salary, status)
                    - Addresses(country, region, post_code, city, address_1, address_2)
                    - Positions(position_name, description)
                    - Departments(department_name, description)
                    - LeaveRequests(employee_id, start_date, end_date, leave_type, status)
                    - Shifts(shift_name, start_time, end_time)
                    - EmployeeShifts(employee_id, shift_id, date)
                    - PerformanceReviews(employee_id, review_date, score, comment)

                    STÁTUSZOK/TÍPUSOK:
                    - Employees - status [active, inactive, pending]
                    - LeaveRequests - leave_type [paid_leave, sick_leave, unpaid_leave, business_trip, training, unexcused_absence], status [approved, rejected, pending]

                    SZABÁLYOK:
                    1. ALIASOK (KÖTELEZŐ): Minden számított mezőnek (COUNT, SUM, AVG) adj nevet az 'AS' használatával! (Pl: SELECT COUNT(*) AS Darabszam)
                    2. KERESÉS: Csak azokat az oszlopokat használd a WHERE feltételben, amelyekre a felhasználó konkrétan rákérdezett.
                    3. JOIN: Használd a táblák közötti logikai kapcsolatokat. ID-t sose írj!
                    4. Csak nyers T-SQL SELECT-et adj vissza, szöveg vagy kódblokk nélkül!
                    5. Csak a megnevezett táblákban kereshetsz, ha más a kérdés akkor nincs válasz!
                    6. Egy alkalmazott nevét a first_name és last_name oszlopokból kapod."
                },
                new { role = "user", content = request.Prompt }
            },
            temperature = 0
        };

        var sqlResponse = await _httpClient.PostAsJsonAsync(url, sqlPayload);
        if (!sqlResponse.IsSuccessStatusCode) return BadRequest($"Groq SQL hiba: {await sqlResponse.Content.ReadAsStringAsync()}");

        using var sqlJsonDoc = await sqlResponse.Content.ReadFromJsonAsync<JsonDocument>();
        string sqlQuery = sqlJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        
        sqlQuery = sqlQuery.Replace("```sql", "").Replace("```", "").Trim();

        try 
        {
            using var connection = new SqlConnection(_connectionString);
            var dbResult = await connection.QueryAsync(sqlQuery);

            string serializedData = JsonSerializer.Serialize(dbResult);
            
            var summaryPayload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] {
                    new { role = "system", content = "Te egy NavetraERP HR asszisztens vagy. Az adatbázisból kapott JSON alapján válaszolj. A JSON-ban az oszlopnevek (aliasok) segítenek értelmezni a számokat. Légy rövid és lényegretörő!" },
                    new { role = "user", content = $"Kérdés: {request.Prompt} \n Adatok: {serializedData}" }
                },
                temperature = 0.5
            };

            var summaryResponse = await _httpClient.PostAsJsonAsync(url, summaryPayload);
            string finalAnswer = "Az adatokat sikerült lekérni, de az elemzés elakadt.";

            if (summaryResponse.IsSuccessStatusCode)
            {
                using var summaryJsonDoc = await summaryResponse.Content.ReadFromJsonAsync<JsonDocument>();
                finalAnswer = summaryJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            }

            return Ok(new { 
                query = sqlQuery, 
                data = dbResult, 
                answer = finalAnswer 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "SQL hiba", details = ex.Message, query = sqlQuery });
        }
    }

    [Authorize]
    [HttpPost("procurement")]
    public async Task<IActionResult> AnalyzeProcurement([FromBody] AiRequest request)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        var url = "https://api.groq.com/openai/v1/chat/completions";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var sqlPayload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] {
                new { 
                    role = "system", 
                    content = @"Te egy NavetraERP Beszerzés Modul szakértő SQL generátor vagy. 
                    TÁBLÁK:
                    - Suppliers(id, name, tax_number, eu_tax_number, bank_account_number, contact_person, email, phone_number, address_id)
                    - Addresses(id, country, region, post_code, city, address_1, address_2)
                    - PurchaseOrders(id, supplier_id, order_date, expected_delivery_date, status, total_amount)
                    - PurchaseOrderItems(purchase_order_id, product_id, quantity_ordered, price_per_unit, discount, tax_rate)
                    - GoodsReceipts(id, purchase_order_id, warehouse_id, receipt_date, received_by_employee_id)
                    - GoodsReceiptItems(goods_receipt_id, product_id, quantity_received, batch_number)
                    - Products(id, name, unit)
                    - Warehouses(id, name, address_id)
                    - Employees(id, first_name, last_name)

                    STÁTUSZOK:
                    - PurchaseOrders - status [pending, received, delayed, cancelled]

                    SZABÁLYOK:
                    1. ALIASOK (KÖTELEZŐ): Minden számított mezőnek (COUNT, SUM, AVG) adj nevet az 'AS' használatával! (Pl: SELECT COUNT(*) AS Darabszam)
                    2. KERESÉS: Csak azokat az oszlopokat használd a WHERE feltételben, amelyekre a felhasználó konkrétan rákérdezett.
                    3. JOIN: Használd a táblák közötti logikai kapcsolatokat. ID-t sose írj!
                    4. Csak nyers T-SQL SELECT-et adj vissza, szöveg vagy kódblokk nélkül!
                    5. Csak a megnevezett táblákban kereshetsz, ha más a kérdés akkor nincs válasz!
                    6. Ha egy bizonylat számára kérdeznek az a PurchaseOrder-nél CONCAT('PO-', FORMAT(id, '00000')), a GoodsReceipt-nél CONCAT('GR-', FORMAT(id, '00000')). Midnig a szám 5 karatker hosszú legyen."
                },
                new { role = "user", content = request.Prompt }
            },
            temperature = 0
        };

        var sqlResponse = await _httpClient.PostAsJsonAsync(url, sqlPayload);
        if (!sqlResponse.IsSuccessStatusCode) return BadRequest($"Groq SQL hiba: {await sqlResponse.Content.ReadAsStringAsync()}");

        using var sqlJsonDoc = await sqlResponse.Content.ReadFromJsonAsync<JsonDocument>();
        string sqlQuery = sqlJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        
        sqlQuery = sqlQuery.Replace("```sql", "").Replace("```", "").Trim();

        try 
        {
            using var connection = new SqlConnection(_connectionString);
            var dbResult = await connection.QueryAsync(sqlQuery);

            string serializedData = JsonSerializer.Serialize(dbResult);
            
            var summaryPayload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] {
                    new { role = "system", content = "Te egy NavetraERP Beszerzés asszisztens vagy. Az adatbázisból kapott JSON alapján válaszolj. A JSON-ban az oszlopnevek (aliasok) segítenek értelmezni a számokat. Légy rövid és lényegretörő!" },
                    new { role = "user", content = $"Kérdés: {request.Prompt} \n Adatok: {serializedData}" }
                },
                temperature = 0.5
            };

            var summaryResponse = await _httpClient.PostAsJsonAsync(url, summaryPayload);
            string finalAnswer = "Az adatokat sikerült lekérni, de az elemzés elakadt.";

            if (summaryResponse.IsSuccessStatusCode)
            {
                using var summaryJsonDoc = await summaryResponse.Content.ReadFromJsonAsync<JsonDocument>();
                finalAnswer = summaryJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            }

            return Ok(new { 
                query = sqlQuery, 
                data = dbResult, 
                answer = finalAnswer 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "SQL hiba", details = ex.Message, query = sqlQuery });
        }
    }

    [Authorize]
    [HttpPost("sales")]
    public async Task<IActionResult> AnalyzeSales([FromBody] AiRequest request)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        var url = "https://api.groq.com/openai/v1/chat/completions";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var sqlPayload = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] {
                new { 
                    role = "system", 
                    content = @"Te egy NavetraERP Értékesítés Modul szakértő SQL generátor vagy. 
                    TÁBLÁK:
                    - Customers(id, name, tax_number, eu_tax_number, bank_account_number, email, phone_number, billing_address_id, shipping_address_id)
                    - Addresses(id, country, region, post_code, city, address_1, address_2)
                    - SalesOrders(id, supplier_id, order_date, required_delivery_date, status, total_amount)
                    - SalesOrderItems(sales_order_id, product_id, quantity_ordered, unit_price, discount, tax_rate, shipped_quantity)
                    - Invoices(id, sales_order_id, invoice_date, due_date, tota_amount, paid_amount, status)
                    - InvoiceItems(invoice_id, product_id, quantity, unit_price, tax_rate)
                    - Products(id, name, unit)
                    - Warehouses(id, name, address_id)

                    STÁTUSZOK:
                    - SalesOrders - status [draft, confirmed, delivered, invoiced, closed]
                    - Invoices - status [draft, issued, paid]

                    SZABÁLYOK:
                    1. ALIASOK (KÖTELEZŐ): Minden számított mezőnek (COUNT, SUM, AVG) adj nevet az 'AS' használatával! (Pl: SELECT COUNT(*) AS Darabszam)
                    2. KERESÉS: Csak azokat az oszlopokat használd a WHERE feltételben, amelyekre a felhasználó konkrétan rákérdezett.
                    3. JOIN: Használd a táblák közötti logikai kapcsolatokat. ID-t sose írj!
                    4. Csak nyers T-SQL SELECT-et adj vissza, szöveg vagy kódblokk nélkül!
                    5. Csak a megnevezett táblákban kereshetsz, ha más a kérdés akkor nincs válasz!
                    6. Ha egy bizonylat számára kérdeznek az a SalesOrder-nél CONCAT('SO-', FORMAT(id, '00000')), a Invoice-nél CONCAT('SZ-', FORMAT(id, '00000')). Midnig a szám 5 karatker hosszú legyen.
                    7. A státuszokat és típusokat fordítsd le."
                },
                new { role = "user", content = request.Prompt }
            },
            temperature = 0
        };

        var sqlResponse = await _httpClient.PostAsJsonAsync(url, sqlPayload);
        if (!sqlResponse.IsSuccessStatusCode) return BadRequest($"Groq SQL hiba: {await sqlResponse.Content.ReadAsStringAsync()}");

        using var sqlJsonDoc = await sqlResponse.Content.ReadFromJsonAsync<JsonDocument>();
        string sqlQuery = sqlJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        
        sqlQuery = sqlQuery.Replace("```sql", "").Replace("```", "").Trim();

        try 
        {
            using var connection = new SqlConnection(_connectionString);
            var dbResult = await connection.QueryAsync(sqlQuery);

            string serializedData = JsonSerializer.Serialize(dbResult);
            
            var summaryPayload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] {
                    new { role = "system", content = "Te egy NavetraERP Értékesítés asszisztens vagy. Az adatbázisból kapott JSON alapján válaszolj. A JSON-ban az oszlopnevek (aliasok) segítenek értelmezni a számokat. Légy rövid és lényegretörő!" },
                    new { role = "user", content = $"Kérdés: {request.Prompt} \n Adatok: {serializedData}" }
                },
                temperature = 0.5
            };

            var summaryResponse = await _httpClient.PostAsJsonAsync(url, summaryPayload);
            string finalAnswer = "Az adatokat sikerült lekérni, de az elemzés elakadt.";

            if (summaryResponse.IsSuccessStatusCode)
            {
                using var summaryJsonDoc = await summaryResponse.Content.ReadFromJsonAsync<JsonDocument>();
                finalAnswer = summaryJsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            }

            return Ok(new { 
                query = sqlQuery, 
                data = dbResult, 
                answer = finalAnswer 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "SQL hiba", details = ex.Message, query = sqlQuery });
        }
    }
}

public class AiRequest 
{ 
    public string Prompt { get; set; } = string.Empty; 
}