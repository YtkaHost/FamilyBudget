using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FamilyBudget.Integration
{
    public class SecurityApiClient
    {
        public const string DefaultUrl = "https://localhost:7030";

        private readonly HttpClient _http;

        public string BaseUrl { get; }

        public SecurityApiClient(string baseUrl = DefaultUrl)
        {
            BaseUrl = baseUrl.TrimEnd('/');

            _http = new HttpClient { BaseAddress = new Uri(BaseUrl + "/"), Timeout = TimeSpan.FromSeconds(10) };
        }

        public LoginResult Login(string username, string password)
        {
            (HttpStatusCode status, string body) = Send(HttpMethod.Post, "api/Users/login", null,
                new { username, password });

            if (status == HttpStatusCode.OK)
                return ParseLogin(body);

            if (status == HttpStatusCode.NotFound)
                throw new InvalidOperationException("Користувача з таким логіном не існує.");
            if (status == HttpStatusCode.BadRequest)
                throw new InvalidOperationException("Невірний пароль.");

            throw CommonError(status, body);
        }

        public void Register(string adminToken, string username, string password, string role)
        {
            (HttpStatusCode status, string body) = Send(HttpMethod.Post, "api/Users/register", adminToken,
                new { username, password, role });

            if (status == HttpStatusCode.OK)
                return;

            if (status == HttpStatusCode.Conflict)
                throw new InvalidOperationException("Користувач з таким логіном уже існує.");

            throw CommonError(status, body);
        }

        public List<ApiUser> GetUsers(string adminToken)
        {
            (HttpStatusCode status, string body) = Send(HttpMethod.Get, "api/Users", adminToken, null);

            if (status == HttpStatusCode.OK)
                return ParseUsers(body);

            throw CommonError(status, body);
        }

        public void SetRole(string adminToken, string username, string role)
        {
            (HttpStatusCode status, string body) = Send(HttpMethod.Put, "api/Users/role/set", adminToken,
                new { username, role });

            if (status == HttpStatusCode.OK)
                return;

            if (status == HttpStatusCode.NotFound)
                throw new InvalidOperationException("Користувача з таким логіном не існує.");
            if (status == HttpStatusCode.BadRequest)
                throw new InvalidOperationException("Такої ролі на сервері немає. Роль з'являється, коли її вказують при створенні користувача.");

            throw CommonError(status, body);
        }

        public void ChangePassword(string userToken, string oldPassword, string newPassword)
        {
            (HttpStatusCode status, string body) = Send(HttpMethod.Put, "api/Users/password/change", userToken,
                new { oldPassword, newPassword });

            if (status == HttpStatusCode.OK)
                return;

            if (status == HttpStatusCode.BadRequest)
                throw new InvalidOperationException("Не вдалося змінити пароль: " + ShortText(body));

            throw CommonError(status, body);
        }


        private (HttpStatusCode, string) Send(HttpMethod method, string path, string? token, object? body)
        {
            var request = new HttpRequestMessage(method, path);

            if (token != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = _http.Send(request);
                string text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return (response.StatusCode, text);
            }
            catch (HttpRequestException ex)
            {
                string hint = ex.InnerException is System.Security.Authentication.AuthenticationException
                    ? " Схоже, браузер/система не довіряє сертифікату розробника: виконайте dotnet dev-certs https --trust."
                    : " Перевірте, що сервіс Security запущено (dotnet run у папці Security).";

                throw new InvalidOperationException("Не вдалося зв'язатися із сервісом Security (" + BaseUrl + "): "
                    + ex.Message.TrimEnd('.') + "." + hint);
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException("Сервіс Security не відповів за 10 секунд. Спробуйте пізніше.");
            }
        }

        private InvalidOperationException CommonError(HttpStatusCode status, string body)
        {
            if (status == HttpStatusCode.Unauthorized)
                return new InvalidOperationException("Сервіс не прийняв токен (401). Вийдіть з облікового запису і увійдіть знову.");
            if (status == HttpStatusCode.Forbidden)
                return new InvalidOperationException("Недостатньо прав: цю дію може виконати лише користувач з роллю Admin.");

            return new InvalidOperationException("Сервіс Security повернув помилку " + (int)status + ": " + ShortText(body));
        }

        private string ShortText(string body)
        {
            string text = body.Trim();

            if (text == "")
                return "(без пояснення)";

            return text.Length > 200 ? text.Substring(0, 200) + "..." : text;
        }

        private LoginResult ParseLogin(string json)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(json);

                var result = new LoginResult();
                result.Token = GetString(document.RootElement, "token");
                result.Role = GetString(document.RootElement, "role");

                if (result.Token == "")
                    throw new InvalidOperationException("у відповіді немає токена");

                return result;
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                throw new InvalidOperationException("Сервіс Security повернув відповідь у неочікуваному форматі (" + ex.Message + ").");
            }
        }

        private List<ApiUser> ParseUsers(string json)
        {
            var users = new List<ApiUser>();

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);

                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    JsonElement source = item;

                    if (!HasProperty(item, "userName") && HasProperty(item, "result"))
                        source = FindProperty(item, "result");

                    var user = new ApiUser();
                    user.UserName = GetString(source, "userName");
                    user.Role = GetString(source, "role");
                    users.Add(user);
                }
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                throw new InvalidOperationException("Сервіс Security повернув список користувачів у неочікуваному форматі (" + ex.Message + ").");
            }

            return users;
        }

        private bool HasProperty(JsonElement element, string name)
        {
            // API може повертати назви полів з іншим регістром.
            return element.ValueKind == JsonValueKind.Object
                && element.EnumerateObject().Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private JsonElement FindProperty(JsonElement element, string name)
        {
            return element.EnumerateObject().First(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)).Value;
        }

        private string GetString(JsonElement element, string name)
        {
            if (!HasProperty(element, name))
                return "";

            return FindProperty(element, name).GetString() ?? "";
        }
    }
}
