using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Database;
using WebApplication2.Helper;
using WebApplication2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                        builder.WithOrigins("http://localhost:5173", "https://localhost:5173")// ��������� ������� � ������ ������
                                .AllowAnyMethod() // ��������� ����� HTTP-������ (GET, POST � �.�.)
                               .AllowAnyHeader()
                               .AllowCredentials();  // ��������� ����� ���������
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();
            app.Urls.Add("http://*:5000");
            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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

                // ���������, ��� �������� ����������
                if (document == null)
                {
                    return Results.NotFound("�������� �� ������.");
                }

                // ���������, ��� ���� ����������
                if (!System.IO.File.Exists(document.FilePath))
                {
                    return Results.NotFound("���� �� ������.");
                }

                // ���������, ��� ���� �� ������
                var fileInfo = new FileInfo(document.FilePath);
                if (fileInfo.Length == 0)
                {
                    return Results.NotFound("���� ������.");
                }

                // ��������� ���� ��� ������
                var fileStream = System.IO.File.OpenRead(document.FilePath);

                // ��������� ��������� Content-Disposition
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = document.FileName, // ��� �����
                    Inline = false, // ���������, ��� ���� ������ �����������, � �� ����������� � ��������
                };
                httpContext.Response.Headers["Content-Disposition"] = contentDisposition.ToString();

                // ���������� ���� � ����������� �����������
                return Results.File(fileStream, "application/pdf", document.FileName);
            });

            //post
            app.MapPost("api/createUser", async (ApplicationDBContext ctx, [FromBody] UserDTO userDTO ,  PasswordGeneration generator, SendSMS sender, Validation val) =>
            {
                // ��������, ��� ������ user �� null
                if (userDTO == null)
                {
                    return Results.BadRequest("������ ������������ �� �������������.");
                }



                userDTO.Phone = val.ValidPhone(userDTO.Phone);
                if (ctx.Users.FirstOrDefault(u => u.Phone == userDTO.Phone) != null)
                {
                    return Results.Conflict("�� ��� ����������������, ���������� �������� ������� �����������");
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

                    await ctx.SaveChangesAsync(); // ���������, ����� �������� Id ������������
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
                // ���������, ���������� �� ������������
                var user = await ctx.Users.FindAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("������������ �� ������.");
                }

                // ���������, ���� �� ����� � �������
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest("��� ������ ��� ��������.");
                }

                var form = await request.ReadFormAsync();
                string title = form["title"];
                var files = form.Files;

                // ����� ��� �������� ������
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // ������������ ������ ����
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // ���������� ���������� ��� �����
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // ��������� ���� �� �������
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // ��������� ���������� � ����� � ���� ������
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

                // ��������� ��������� � ���� ������
                await ctx.SaveChangesAsync();

                return Results.Ok("����� ������� ���������.");
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
                    if (!user.Password.IsNullOrEmpty()) oldUser.Password = user.Password; // ������ ����������� ������ ���� �������
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
                    return Results.NotFound("�������� �� ������.");
                }

                // ������� ���� � �������
                if (System.IO.File.Exists(document.FilePath))
                {
                    System.IO.File.Delete(document.FilePath);
                }

                // ������� ������ �� ���� ������
                ctx.Documents.Remove(document);
                await ctx.SaveChangesAsync();

                return Results.Ok("�������� ������� ������.");
            });

            app.Run();
        }
      

    }
}