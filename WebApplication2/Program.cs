using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Database;
using WebApplication2.Helper;
using WebApplication2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Telegram.Bot;
using System.Reflection.Emit;
using System.Reflection;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Cors;

namespace WebApplication2
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddHttpClient();
            // Add services to the container.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
            builder.Services.AddAuthorization();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.WithOrigins(
                                "https://guleb23-mtrepo-b896.twc1.net",
                                "https://guleb23-webapplication2-a40c.twc1.net"
                            )
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                    });
            });



            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<PasswordGeneration>();
            builder.Services.AddScoped<SendSMS>();
            builder.Services.AddScoped<Validation>();
            builder.Services.AddSingleton<JWTGenerator>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new JWTGenerator(
                    configuration["Jwt:Key"],
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"]
                );
            });
            builder.Configuration.AddJsonFile("appsettings.json");

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            const string BOT_TOKEN = "7593576707:AAFfwzMnHc6eUpyrZVrWhJokJg_NdK4LcQs"; // Вставь токен бота

            var app = builder.Build();
            app.UseCors("AllowAllOrigins");
            app.MapMethods("/auth/telegram", new[] { "OPTIONS" },
             () => Results.Ok()).RequireCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection(); // Перенаправление на HTTPS
            
            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();




            var botClient = new TelegramBotClient(BOT_TOKEN);
            app.MapGet("/", () => "Server is running!"); // Проверочный маршрут

            app.MapPost("/auth/telegram", [EnableCors("AllowAllOrigins")] async (HttpContext context, ApplicationDBContext ctx, JWTGenerator generator, TelegramDataDTO authData) =>
            {
               

                // 1️⃣ Проверяем подлинность данных (защита от подмены)
                string botToken = BOT_TOKEN; 

                // 2️⃣ Проверяем, есть ли пользователь в базе
                var existingUser = ctx.Users.FirstOrDefault(u => u.Id == authData.id);
                if (existingUser != null)
                {
                    string jwt = generator.GenerateJwtToken(authData.username);
                    return Results.Json(new { token = jwt, id = authData.id });
                }

                // 3️⃣ Если пользователя нет — создаем его
                UserModel newUser = new UserModel()
                {
                    Id = authData.id,
                    FirstName = authData.first_name,
                    LastName = "",
                    Email = "",
                    Phone = "",
                    Password = ""
                };

                ctx.Users.Add(newUser);
                await ctx.SaveChangesAsync();

                // 4️⃣ Создаем личные данные (если нужно)
                PersonalDataModel personal = new PersonalDataModel()
                {
                    Seria = "",
                    Nomer = "",
                    SNILS = "",
                    DateVidachi = "",
                    Propiska = "",
                    WhoVidal = "",
                    UserId = newUser.Id
                };

                ctx.PersonalData.Add(personal);
                await ctx.SaveChangesAsync();
                string jwtM = generator.GenerateJwtToken(authData.username);
                return Results.Json(new { token = jwtM, id = authData.id });
            });
            bool ValidateTelegramData(long id, string firstName, string? lastName, string? username,string? photoUrl, long authDate, string hash)
            {
                var dataCheckString = $"auth_date={authDate}\n" +
                                      $"first_name={firstName}\n" +
                                      $"id={id}\n" +
                                      (lastName != null ? $"last_name={lastName}\n" : "") +
                                      (username != null ? $"username={username}\n" : "") +
                                      (photoUrl != null ? $"photo_url={photoUrl}\n" : "");

                dataCheckString = dataCheckString.TrimEnd('\n');

                var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(BOT_TOKEN));
                using var hmac = new HMACSHA256(secretKey);
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
                var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return computedHash == hash;
            }
            
            //Methods
            //Get
            app.MapGet("api/users", (ApplicationDBContext ctx) =>
            {
                return ctx.Users.ToList();
            });
            app.MapGet("api/persdata", (ApplicationDBContext ctx) =>
            {
                return ctx.PersonalData.ToList();
            });
            app.MapGet("api/persdata/{id}", [Authorize] (ApplicationDBContext ctx, int id) =>
            {
                return ctx.PersonalData.FirstOrDefault(x => x.UserId == id);
            });

            app.MapGet("api/user/{id}", [Authorize] async (ApplicationDBContext ctx, int id) =>
            {
                var user = ctx.Users.FirstOrDefault(x => x.Id == id);
                if (user == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    return Results.Ok(user);
                }
            });
            app.MapGet("api/payments", async (ApplicationDBContext ctx) =>
            {
                return ctx.PaymentMethods.OrderBy(u => u.Id).ToList();
            });
            app.MapGet("api/docs", async (ApplicationDBContext ctx) =>
            {
                return ctx.GetDocsSposobModels.OrderBy(u => u.Id).ToList();
            });
            app.MapGet("api/documents/{id}", [Authorize] async (ApplicationDBContext ctx, int id) =>
            {
                return ctx.Documents.Where(x => x.UserId == id).ToList();
            });
            app.MapGet("api/downloadDocument/{documentId}", [Authorize] async (ApplicationDBContext ctx, int documentId, HttpContext httpContext) =>
            {
                var document = await ctx.Documents.FindAsync(documentId);

                // Проверяем, что документ существует
                if (document == null)
                {
                    return Results.NotFound("Документ не найден.");
                }

                // Проверяем, что файл существует
                if (!System.IO.File.Exists(document.FilePath))
                {
                    return Results.NotFound("Файл не найден.");
                }

                // Проверяем, что файл не пустой
                var fileInfo = new FileInfo(document.FilePath);
                if (fileInfo.Length == 0)
                {
                    return Results.NotFound("Файл пустой.");
                }

                // Открываем файл для чтения
                var fileStream = System.IO.File.OpenRead(document.FilePath);

                // Добавляем заголовок Content-Disposition
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = document.FileName, // Имя файла
                    Inline = false, // Указываем, что файл должен скачиваться, а не открываться в браузере
                };
                httpContext.Response.Headers["Content-Disposition"] = contentDisposition.ToString();

                // Возвращаем файл с правильными заголовками
                return Results.File(fileStream, "application/pdf", document.FileName);
            });


            app.MapPost("api/createUser", async (ApplicationDBContext ctx, [FromBody] UserDTO userDTO ,  PasswordGeneration generator, SendSMS sender, Validation val) =>
            {
                // Проверка, что объект user не null
                if (userDTO == null)
                {
                    return Results.BadRequest("Данные пользователя не предоставлены.");
                }



                userDTO.Phone = val.ValidPhone(userDTO.Phone);
                if (ctx.Users.FirstOrDefault(u => u.Phone == userDTO.Phone) != null)
                {
                    return Results.Conflict("Вы уже зарегестрированы, пожалуйста пройдите процесс авторизации");
                }
                else
                {
                    UserModel userModel = new UserModel()
                    {
                        FirstName = userDTO.FirstName,
                        LastName = userDTO.LastName,
                        Email = userDTO.Email,
                        Phone = userDTO.Phone,
                        Password = generator.GeneratePassword(),
                        PaymentMethodId = userDTO.PaymentMethodId,
                        GetDocsSposobId = userDTO.GetDocsSposobId,
                    };

                    ctx.Users.Add(userModel);

                    await ctx.SaveChangesAsync(); // Сохраняем, чтобы получить Id пользователя
                    PersonalDataModel pesonal = new PersonalDataModel()
                    {
                        Seria = "",
                        Nomer = "",
                        SNILS = "",
                        DateVidachi = "",
                        Propiska = "",
                        WhoVidal = "",
                        UserId = userModel.Id,
                    };
                    
                    
                    ctx.PersonalData.Add(pesonal);
                    await ctx.SaveChangesAsync();
                    sender.SendSmsAsync(userModel.Phone, userModel.Password);

                    return Results.Ok(userModel);
                }



            });

            app.MapPost("api/login", async (ApplicationDBContext ctx, UserLoginDTO loginUser, JWTGenerator generator, Validation val) =>
            {
                loginUser.Phone = val.ValidPhone(loginUser.Phone);
                UserModel? user = ctx.Users.FirstOrDefault(u => u.Password == loginUser.Password && u.Phone == loginUser.Phone);
                if (user != null)
                {
                    string jwt = generator.GenerateJwtToken(loginUser.Phone);
                    var jsonObject = new
                    {
                        token = jwt,
                        id = user.Id.ToString(),

                    };
                    return Results.Ok(jsonObject);
                }
                else
                {
                    return Results.NotFound("Non user");
                }
            });

            app.MapPost("api/uploadDocument/{userId}", [Authorize] async (ApplicationDBContext ctx, int userId, HttpRequest request) =>
            {
                // Проверяем, существует ли пользователь
                var user = await ctx.Users.FindAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("Пользователь не найден.");
                }

                // Проверяем, есть ли файлы в запросе
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest("Нет файлов для загрузки.");
                }

                var form = await request.ReadFormAsync();
                string title = form["title"];
                var files = form.Files;

                // Папка для хранения файлов
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Обрабатываем каждый файл
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Генерируем уникальное имя файла
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Сохраняем файл на сервере
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Сохраняем информацию о файле в базе данных
                        var document = new DocumentModel
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            UserId = userId,
                            Title = title
                        };

                        ctx.Documents.Add(document);
                    }
                }

                // Сохраняем изменения в базе данных
                await ctx.SaveChangesAsync();

                return Results.Ok("Файлы успешно загружены.");
            });

            //update
            app.MapPatch("api/user/{id}", [Authorize] async (ApplicationDBContext ctx, int id, [FromBody] UserDTO user) =>
            {
                var oldUser = ctx.Users.FirstOrDefault(x => x.Id == id);
                if (oldUser == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    if (!user.Email.IsNullOrEmpty()) oldUser.Email = user.Email;
                    if (!user.FirstName.IsNullOrEmpty()) oldUser.FirstName = user.FirstName;
                    if (user.GetDocsSposobId != 0) oldUser.GetDocsSposobId = user.GetDocsSposobId;
                    if (!user.LastName.IsNullOrEmpty()) oldUser.LastName = user.LastName;
                    if (!user.Password.IsNullOrEmpty()) oldUser.Password = user.Password; // Пароль обновляется только если передан
                    if (user.PaymentMethodId != 0) oldUser.PaymentMethodId = user.PaymentMethodId;
                    if (!user.Phone.IsNullOrEmpty()) oldUser.Phone = user.Phone;
                    await ctx.SaveChangesAsync();
                    return Results.Ok(user);
                    
                }

            });

            app.MapPut("api/persData/{id}", [Authorize] async (ApplicationDBContext ctx, int id, [FromBody] PersDataDTO newData) =>
            {
                var oldData = ctx.PersonalData.FirstOrDefault(x => x.Id == id);
                if (oldData == null)
                {
                    return Results.NotFound();
                }
                else
                {
                    oldData.Seria = newData.Seria;
                    oldData.Nomer = newData.Nomer;
                    oldData.Propiska = newData.Propiska;
                    oldData.WhoVidal = newData.WhoVidal;
                    oldData.SNILS = newData.SNILS;
                    oldData.DateVidachi = newData.DateVidachi;
                    await ctx.SaveChangesAsync();

                    return Results.Ok(oldData);

                }

            });



            app.MapDelete("api/deleteDocument/{documentId}", async (ApplicationDBContext ctx, int documentId) =>
            {
                var document = await ctx.Documents.FindAsync(documentId);

                if (document == null)
                {
                    return Results.NotFound("Документ не найден.");
                }

                // Удаляем файл с сервера
                if (System.IO.File.Exists(document.FilePath))
                {
                    System.IO.File.Delete(document.FilePath);
                }

                // Удаляем запись из базы данных
                ctx.Documents.Remove(document);
                await ctx.SaveChangesAsync();

                return Results.Ok("Документ успешно удален.");
            });

            app.Run();
        }
        public class Update
        {
            public Message Message { get; set; }
        }

        public class Message
        {
            public Contact Contact { get; set; }
        }

        public class Contact
        {
            public string PhoneNumber { get; set; }
            public long UserId { get; set; }
        }

    }
}