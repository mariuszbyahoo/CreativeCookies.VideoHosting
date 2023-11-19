﻿using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DAL.Repositories;
using CreativeCookies.VideoHosting.DAL.Repositories.OAuth;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCookies.VideoHosting.DAL.Config
{
    public static class DALExtensions
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });

            services.AddDefaultIdentity<MyHubUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.ProviderMap[TokenOptions.DefaultAuthenticatorProvider] = new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<MyHubUser>));
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddHangfire(conf => conf.UseSqlServerStorage(connectionString));

            services.AddScoped<IInvoiceNumsRepository, InvoiceNumsRepository>();
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IFilmsRepository, FilmsRepository>();
            services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IConnectAccountsRepository, ConnectAccountsRepository>();
            services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IClientStore, ClientStore>();
            services.AddScoped<IAboutPageRepository, AboutPageRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IRegulationsRepository, RegulationsRepository>();

            return services;
        }

        public static WebApplication MigrateAndPopulateDatabase(this WebApplication app, string adminEmail)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MyHubUser>>();
                var regulationsRepository = scope.ServiceProvider.GetRequiredService<IRegulationsRepository>();
                // Ensure the roles exist
                var roles = new[] { "admin", "subscriber", "nonsubscriber" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }

                // Create an admin user
                var adminUser = userManager.FindByEmailAsync(adminEmail)?.Result;

                if (adminUser == null)
                {
                    adminUser = new MyHubUser { UserName = adminEmail, Email = adminEmail };
                    adminUser.EmailConfirmed = true;
                    var result = userManager.CreateAsync(adminUser, "Pass123$").Result;
                    if (result.Succeeded)
                    {
                        // HACK: Send an email about creation of the user to adminEmail
                        userManager.AddToRoleAsync(adminUser, "admin").Wait();
                    }
                }

                regulationsRepository.UpdatePrivacyPolicy(new DTOs.Regulations.WebsitePrivacyPolicyDTO()
                {
                    HtmlContent = "<p><br></p><h2 class=\"ql-align-right\"><span style=\"background-color: transparent; color: rgb(0, 0, 0);\">&nbsp;&nbsp;&nbsp;&nbsp;19.11.2023 r.&nbsp;</span></h2><h1 class=\"ql-align-center\"><span style=\"background-color: transparent; color: rgb(0, 0, 0);\">REGULAMIN SERWISU [Nazwa Serwisu]</span></h1><p><br></p><p class=\"ql-align-center\"><span style=\"background-color: transparent; color: rgb(0, 0, 0);\">Niniejszy Regulamin określa ogólne warunki korzystania z Usług dostarczanych za pośrednictwem Serwisu:</span></p><ol><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">&nbsp;Abonament – cykliczna opłata, ponoszona przez Użytkownika z tytułu umożliwienia mu przez Operatora Serwisu dostępu do Konta i funkcjonalności świadczonych za pośrednictwem Serwisu.</strong></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Konto&nbsp;</strong><span style=\"background-color: transparent;\">–&nbsp;dostęp do funkcjonalności Serwisu dla Użytkownika po dokonaniu Rejestracji.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Okres Rozliczeniowy – okres rozliczeniowy Umowy Konta. Wynosi miesiąc kalendarzowy.&nbsp;</strong></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Operator Serwisu</strong><span style=\"background-color: transparent;\"> –[xxx] z siedzibą w [xxx] [kod pocztowy] przy ul. [xxx] wpisana do rejestru przedsiębiorców prowadzonego przez Sąd Rejonowy w [xxx], [xxx] Wydział Gospodarczy Krajowego Rejestru Sądowego, pod numerem KRS: [xxx] NIP: [xxx], posiadająca kapitał zakładowy w wysokości [xxx] złotych. </span><strong style=\"background-color: transparent; color: rgb(178, 178, 0);\">Ewentualnie wykasować powyższe i wpisać dane jednoosobowej działalności gospodarczej z CEIDG</strong></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Pośrednik płatności </strong><span style=\"background-color: transparent;\">– platforma Stripe Inc. 354 Oyster Point Boulevard, South San Francisco, California, 94080, USA, pośrednicząca w płatności w przypadku dokonania zakupu przez Użytkownika dostępu do treści z Serwisu.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Polityka Prywatności</strong><span style=\"background-color: transparent;\"> – polityka prywatności i plików cookies zawierająca postanowienia dotyczące przetwarzania danych osobowych przez Operatora Serwisu.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Regulamin </strong><span style=\"background-color: transparent;\">– niniejszy regulamin Serwisu [xxx]. Dokument ten stanowi jednocześnie regulamin świadczenia usług drogą elektroniczną, o którym mowa w art. 8 ust. 1 pkt 1 ustawy o&nbsp;świadczeniu usług drogą elektroniczną z 18 lipca 2002 r.&nbsp;</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Rejestracja </strong><span style=\"background-color: transparent;\">– proces utworzenia Konta Użytkownika, na który składa się podanie danych wymaganych przez Serwis, akceptacja Regulaminu i Polityki Prywatności.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Serwis </strong><span style=\"background-color: transparent;\">– prowadzona przez Operatora Serwisu internetowa platforma online dostępna w domenie [xxx].</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Umowa Konta</strong><span style=\"background-color: transparent;\"> –&nbsp;zawarta na odległość umowa o świadczenie usługi prowadzenia Konta zawierana między Operatorem Serwisu a Użytkownikiem w momencie Rejestracji Konta. Umowa jest zawierana na czas nieokreślony.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Usługi –</strong><span style=\"background-color: transparent;\"> usługi świadczone lub udostępniane przez Operatora Serwisu z wykorzystaniem Serwisu, w szczególności udzielenie Użytkownikowi dostępu do zawartości Serwisu, w tym w postaci możliwości nabycia dostępu do filmów video przez Użytkownika po dokonaniu opłaty Abonamentu.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Użytkownik </strong><span style=\"background-color: transparent;\">– osoba fizyczna, osoba prawna oraz osoba nieposiadająca osobowości prawnej, której ustawa przyznaje zdolność prawną, korzystająca z treści lub usług świadczonych za pośrednictwem Serwisu.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Weryfikacja</strong><span style=\"background-color: transparent;\"> – proces sprawdzający Użytkownika zakładającego Konto w Serwisie przeprowadzany przez Operatora Serwisu polegający na potwierdzeniu przez Użytkownika linku aktywacyjnego wysłanego na adres e-mail podany w trakcie Rejestracji przez Użytkownika.</span></li><li class=\"ql-align-justify\"><strong style=\"background-color: transparent;\">Wykonawca Serwisu </strong><span style=\"background-color: transparent;\">– Creative Cookies sp. z o.o. z siedzibą w Piasecznie,&nbsp;(05-501) przy ul. Dunikowskiego 8, wpisana do rejestru przedsiębiorców prowadzonego przez Sąd Rejonowy dla miasta st. Warszawy w Warszawie, XIV Wydział Gospodarczy Krajowego Rejestru Sądowego, pod numerem KRS: 0000869693, NIP: 1231479701, posiadająca kapitał zakładowy w wysokości 5000,00 złotych.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">1. Postanowienia wstępne</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Niniejszy Regulamin określa zasady korzystania z Serwisu przez Użytkowników. Użytkownik obowiązany jest zapoznać się z treścią Regulaminu i zaakceptować jego postanowienia przed rozpoczęciem korzystania z Serwisu.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W ramach Serwisu Operator Serwisu umożliwia korzystanie z funkcjonalności Serwisu. Główną funkcjonalnością Serwisu jest możliwość nabycia dostępu do treści, w tym filmów umieszczonych w Serwisie przez Operatora.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Korzystając z Serwisu, Użytkownik wyraża zgodę na związanie się warunkami niniejszego Regulaminu. Jeśli Użytkownik nie zgadza się z postanowieniami niniejszego Regulaminu, korzystanie z funkcjonalności Serwisu jest niedozwolone.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Korzystanie z Serwisu jest płatne.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">O ile nie zastrzeżono inaczej, ilekroć mowa jest o adresie e-mail Operatora Serwisu, jest to adres e-mail: [</span><span style=\"background-color: rgb(255, 255, 0);\">xxx</span><span style=\"background-color: transparent;\">]&nbsp;</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">2. Konto Użytkownika</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik może korzystać z funkcjonalności Konta wyłącznie po dokonaniu Rejestracji polegającej na założeniu w Serwisie swojego Konta poprzez wypełnienie odpowiedniego formularza rejestracyjnego w Serwisie.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Do prawidłowego dokonania Rejestracji, o której mowa w pkt 2a konieczne jest spełnienie wymagań określonych w Serwisie, w tym uprzednie zaakceptowanie przez Użytkownika niniejszego Regulaminu oraz Polityki Prywatności.&nbsp;&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Rejestracja Konta wymaga dokonania zapłaty Abonamentu należnego za pierwszy Okres Rozliczeniowy.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Po spełnieniu obowiązku, o którym mowa w pkt 2a, przeprowadzana jest Weryfikacja, w trakcie której na podany przez Użytkownika adres e-mail zostanie wysłane potwierdzenie utworzenia Konta Użytkownika oraz link do jego aktywacji. Rejestracja zostaje zakończona w momencie aktywacji Konta przez Użytkownika. W tym momencie dochodzi do zawarcia Umowy Konta.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik może posiadać tylko jedno Konto Użytkownika w&nbsp;Serwisie. Zasada ta nie ma zastosowania, jeżeli zaistnieje konieczność założenia kolejnego Konta z powodu braku możliwości dostępu do Konta Użytkownika lub innych ważnych przyczyn wskazanych przez Użytkownika, przy czym wszelkie wyłączenia w tym zakresie będą szczegółowo weryfikowane przez Operatora Serwisu, który będzie uprawniony do zawieszenia Konta Użytkownika na czas weryfikacji lub usunięcia Kont w przypadku braku potwierdzenia okoliczności uzasadniających zastosowanie powyższego wyłączenia.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik zobowiązany jest do podania prawdziwych, poprawnych i aktualnych danych podczas procesu Rejestracji oraz korzystania z Serwisu. Użytkownik zobowiązany jest do aktualizacji danych w przypadku ich zmiany.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik zobowiązuje się do zachowania w tajemnicy danych dostępowych do Konta oraz do ich ochrony przed dostępem nieuprawnionych osób trzecich. Użytkownik zobowiązany jest niezwłocznie poinformować Operatora Serwisu w przypadku powzięcia informacji o uzyskaniu przez nieuprawnione osoby trzecie danych dostępowych do Konta Użytkownika i w miarę możliwości niezwłocznie je zmienić.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W celu zapewnienia prawidłowego funkcjonowania usług Serwisu oraz ochrony i zapewnienia bezpieczeństwa osób i podmiotów z niego korzystających, Operator Serwisu zastrzega sobie prawo do przeprowadzenia dodatkowej weryfikacji aktualności i prawdziwości danych podanych przez Użytkownika oraz do żądania od Użytkownika potwierdzenia tożsamości reprezentującej go osoby lub innych niezbędnych danych w sposób wybrany przez Operatora Serwisu. W przypadku bezskutecznej weryfikacji danych lub tożsamości Użytkownika, Operator Serwisu może zawiesić lub zablokować działanie Konta.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">3. Usługi&nbsp;</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W ramach Konta&nbsp;Użytkownik otrzymuje możliwość odpłatnego nabywania dostępu do treści znajdujących się w Serwisie, w tym filmów wideo umieszczanych w Serwisie przez Operatora Serwisu.</span></li><li class=\"ql-align-justify ql-indent-1\"><strong style=\"color: rgb(178, 178, 0);\">TU JEST Miejsce na opisanie wszelkich dodatkowych usług, jakie mogą się w Serwisie dla Użytkownika pojawić. Np. w ramach konta użytkownik może zostać wpisany na listę mailingową do sklepu, czy czegoś innego, za każdym razem należy skonsultować ze specjalistą prawnym jak to zrobić. </strong></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik może za pośrednictwem Serwisu zapisać się do newslettera. Podanie w dedykowanym formularzu danych wymaganych przez Serwis, zaznaczenie opcji „Dołączam do newslettera” oraz zaakceptowanie Regulaminu i Polityki Prywatności jest równoznaczne z wyrażeniem zgody na otrzymywanie od [xxx] newslettera na adres mailowy podany w formularzu. Zgodę, o której mowa w zdaniu poprzednim można wycofać w każdym czasie za pośrednictwem poczty elektronicznej.&nbsp;&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W ramach Serwisu, Użytkownik może dodawać Komentarze. Dodanie Komentarza odbywa się poprzez wpisanie Komentarza w dedykowanym formularzu oraz kliknięcie przycisku „Wyślij”.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Dodanie Komentarza przez Użytkownika jest równoznaczne z oświadczeniem Użytkownika, że Komentarz jest zgodny z rzeczywistością.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności za treść Komentarzy.</span></li></ol><p><br></p><p><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">4. Cena i Płatności</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W ramach procesu Rejestracji, Użytkownik ma obowiązek opłacenia Abonamentu za pierwszy Okres Rozliczeniowy. Cena Abonamentu za pierwszy Okres Rozliczeniowy może się różnić od ceny Abonamentu za kolejne Okresy Rozliczeniowe.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Wysokość ceny za Usługi świadczone Użytkownikowi przez Operatora Serwisu zależy od kwoty podanej w Serwisie w chwili zawarcia Umowy Konta. W przypadku oczywistej omyłki przy podaniu wysokości ceny, Operator Serwisu zastrzega sobie prawo anulowania Rejestracji.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W trakcie trwania Umowy Konta możliwa jest zmiana ceny. Zmiana będzie wiążąca wraz z rozpoczęciem następnego Okresu Rozliczeniowego pod warunkiem poinformowania Użytkownika o zmianie ceny na minimum 7 dni przed rozpoczęciem następnego Okresu Rozliczeniowego&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik dokonuje płatności za Abonament z góry, przed rozpoczęciem Okresu Rozliczeniowego, za który Abonament jest należny. Płatność powinna być dokonana najpóźniej o godzinie 23:59 UTC w dniu upływu poprzedniego Okresu Rozliczeniowego.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Wszelkie płatności Użytkownika dokonywane są za pośrednictwem Pośrednika Płatności zgodnie z warunkami ustalonymi przez Pośrednika Płatności, które znajdują się na jego stronie internetowej pod adresem: </span><a href=\"https://stripe.com/\" rel=\"noopener noreferrer\" target=\"_blank\" style=\"background-color: transparent; color: rgb(5, 99, 193);\">https://stripe.com</a><span style=\"background-color: transparent; color: rgb(5, 99, 193);\">. </span><span style=\"background-color: transparent;\">Pośrednikowi przysługuje część opłaty od każdej transakcji, zgodnie z informacją zawartą na stronie: </span><a href=\"https://stripe.com/en-pl/pricing\" rel=\"noopener noreferrer\" target=\"_blank\" style=\"background-color: transparent; color: rgb(5, 99, 193);\">https://stripe.com/en-pl/pricing</a><span style=\"background-color: transparent;\">.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Płatność uważana jest za dokonaną z chwilą jej zaksięgowania na rachunku bankowym Operatora Serwisu.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Walutą płatności jest złoty (PLN).</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Wszystkie opłaty zawierają podatek VAT, gdzie ma to zastosowanie. Faktura VAT za dokonaną płatność wysyłana jest na wskazany przez Użytkownika na adres e-mail w wersji elektronicznej, na co Użytkownik się zgadza.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności za nieprawidłowe dane podane przez Użytkownika, na podstawie których dokonano płatności.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Niezależnie od części opłaty od transakcji, o której mowa w pkt 4.5, w trakcie dokonywania przez Operatora Serwisu sprzedaży dostępu do treści Użytkownikom za jego pośrednictwem, Pośrednik płatności przekaże Prowizję na rzecz Wykonawcy Serwisu w ustalonej pomiędzy stronami wyosokości od wartości opłaty brutto od każdej transakcji sprzedaży&nbsp;przy wykorzystaniu narzędzi Pośrednika płatności, na zasadach określonych w: </span><a href=\"https://stripe.com/docs/connect/charges#separate-charges-transfers\" rel=\"noopener noreferrer\" target=\"_blank\" style=\"background-color: transparent; color: rgb(5, 99, 193);\">https://stripe.com/docs/connect/charges#separate-charges-transfers</a><span style=\"background-color: transparent; color: rgb(5, 99, 193);\">, </span><span style=\"background-color: transparent;\">zgodnie z odrębnie zawartą umową pomiędzy Operatorem Serwisu a Wykonawcą Serwisu.&nbsp;</span></li></ol><p><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">5. Czas trwania Umowy Konta i Rozwiązanie Umowy Konta.</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><strong style=\"background-color: transparent;\">Umowa Konta Użytkownika zostaje zawarta na czas nieokreślony.</strong></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Użytkownik jest uprawniony do rozwiązania Umowy Konta w każdym czasie. Rozwiązanie Umowy Konta staje się skuteczne z końcem bieżącego Okresu Rozliczeniowego. Użytkownik informuje Operatora Serwisu o rozwiązaniu Konta za pośrednictwem poczty elektronicznej. Operator Serwisu zastrzega sobie możliwość weryfikacji tożsamości i ewentualnego umocowania osoby reprezentującej Użytkownika.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu jest uprawniony do rozwiązania z Użytkownikiem Umowy Konta w każdym czasie. O rozwiązaniu Umowy Konta Operator Serwisu poinformuje Użytkownika za pośrednictwem poczty elektronicznej na adres przypisany do Konta. Rozwiązanie Umowy Konta przez Operatora Serwisu staje się skuteczne z końcem bieżącego Okresu Rozliczeniowego.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Z chwilą rozwiązania Umowy Konta, Użytkownik traci dostęp do informacji i funkcjonalności związanych z dostępem do Konta.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">6. Wymagania techniczne.</span></h2><ol><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Minimalne wymagania techniczne umożliwiające korzystanie z Konta Użytkownika:</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent; color: rgb(0, 0, 0);\">urządzenie posiadające dostęp do sieci Internet z aktualnym systemem operacyjnym;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">zainstalowana oraz prawidłowo skonfigurowana i aktualna wersją przeglądarki internetowej;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">aktywne konto poczty elektronicznej.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">7. Własność intelektualna.</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Serwis objęty jest ochroną praw autorskich. Chroni ona zawartość Serwisu, narzędzia składające się na jego funkcjonalności oraz inne utwory kwalifikowane przez prawo autorskie jako dzieła chronione. Operator Serwisu posiada prawo do korzystania z nich na podstawie odrębnej umowy i są zastrzeżone.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Zabronione jest jakiekolwiek przetwarzanie danych i innych informacji dostępnych w Serwisie w celu udostępnienia ich osobom trzecim w ramach innych serwisów internetowych, jak również poza siecią Internet.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Publikacja jakichkolwiek danych zaczerpniętych z Serwisu powinna zawierać dokładne wskazanie Serwisu. Operator Serwisu zaznacza, że niezależnie od spełnienia warunku określonego w zdaniu poprzednim, zabronione jest publikowanie w celach komercyjnych treści będących własnością Operatora Serwisu na podstawie prawa autorskiego bez uprzedniej pisemnej zgody Operatora Serwisu.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Zabronione jest tworzenie jakichkolwiek narzędzi lub baz danych zawierających dane w zakresie działalności Operatora Serwisu.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">8. Ochrona danych osobowych.</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Serwis działa zgodnie z obowiązującymi przepisami o ochronie danych osobowych. Szczegółowe informacje, zakres, zasady i warunki prywatności dostępne są na stronie: </span><span style=\"background-color: rgb(255, 255, 0);\">……………..</span><span style=\"background-color: transparent;\">&nbsp;</span></li></ol><p><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">9. Odpowiedzialność Operatora Serwisu</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu podejmuje uzasadnione starania, aby zapewnić aktualność, dokładność i nieprzerwaną dostępność zawartości Serwisu.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Chociaż Operator Serwisu dokłada wszelkich starań, aby funkcjonalności dostępne w ramach Serwisu były dostępne w sposób ciągły i działały prawidłowo, nie ponosi odpowiedzialności za żadne konsekwencje wynikające z ich nieprawidłowego działania lub niedostępności w dowolnym momencie, w tym za konsekwencje wynikające z polegania na tych funkcjonalnościach.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności, jeżeli z jakiegokolwiek powodu Serwis będzie niedostępny w dowolnym momencie lub przez dowolny okres.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu dokłada wszelkich starań, aby Serwis był wolny od złośliwego oprogramowania, jednak Operator Serwisu nie ponosi odpowiedzialności za skutki korzystania z Serwisu dla systemu komputerowego lub systemu mobilnego Użytkownika, w tym infrastruktury technicznej i danych.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności za poprawność działania operatorów zapewniających łączność z Serwisem, zarówno po stronie Operatora Serwisu, jak i osób korzystających z Serwisu.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności za prawne, finansowe lub jakiekolwiek inne konsekwencje wykorzystania przez Użytkownika informacji zawartych w Serwisie. Operator Serwisu nie ponosi odpowiedzialności za jakiekolwiek szkody lub straty poniesione przez Użytkownika lub osoby trzecie związane w jakikolwiek sposób z wykorzystaniem tych informacji, w tym w celu podjęcia decyzji w indywidualnej sprawie. Użytkownik korzysta z Serwisu i funkcjonalności dostępnych w ramach Serwisu na własne ryzyko i jest zobowiązany do ich weryfikacji przed wykorzystaniem.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Operator Serwisu nie ponosi odpowiedzialności za szkody (szkodę rzeczywistą oraz szkodę w&nbsp;postaci utraconych korzyści) Użytkowników powstałych w wyniku użytkowania Serwisu.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Serwis może zawierać linki do stron internetowych należących do podmiotów trzecich. Operator Serwisu nie ponosi odpowiedzialności za dostępność treści zawartych na tych stronach, ich jakości, prawidłowości czy ewentualnych skutków skorzystania z linku.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">W przypadku powzięcia informacji przez Użytkownika o błędzie w działaniu Serwisu lub błędzie w danych dostępnych za pomocą Serwisu, Użytkownik jest zobowiązany do poinformowania o tym fakcie Operatora Serwisu, który dołoży starań w celu usunięcia błędnego działania lub błędnych danych.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">10. Zapytania i reklamacje.</span></h2><ol><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Zapytania, wnioski, reklamacje oraz uwagi Użytkownika dotyczące funkcjonowania Serwisu, usług świadczonych za jego pośrednictwem lub Sprzętu, należy kierować na adres e-mail [xxx] Operatora Serwisu. Reklamacja powinna zawierać co najmniej adres e-mail przypisany do Konta oraz dokładny opis okoliczności i nieprawidłowości będących przyczyną reklamacji. Jeżeli podane w reklamacji dane lub informacje nie pozwolą Operatorowi Serwisu na rozpoznanie reklamacji, Operator Serwisu zwróci się do Użytkownika o wyjaśnienie wątpliwości lub o udzielenie dodatkowych informacji drogą elektroniczną, jeżeli będzie to konieczne do rozpoznania reklamacji przez Operatora Serwisu, wskazując dokładnie takie wątpliwości lub wymagane informacje. Jeżeli pomimo tego Użytkownik nie prześle wymaganych danych, Operator Serwisu może pozostawić reklamację bez rozpatrzenia.&nbsp;</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Reklamacja powinna być złożona w terminie 30 dni od zaistnienia reklamowanego zdarzenia. Reklamacje nie będą rozpatrywane po upływie wspomnianego 30-dniowego terminu.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Reklamacje będą rozpatrywane w terminie 14 dni od daty ich otrzymania. Odpowiedź na reklamację wysyłana jest drogą elektroniczną na adres podany w zgłoszeniu reklamacyjnym.</span></li><li class=\"ql-align-justify ql-indent-1\"><span style=\"background-color: transparent;\">Składając reklamację Użytkownik zgadza się na przetwarzanie danych osobowych w zakresie adresu e-mail, z którego została wysłana reklamacja.</span></li></ol><p class=\"ql-align-justify\"><br></p><h2 class=\"ql-align-justify\"><span style=\"background-color: transparent;\">11. Postanowienia końcowe</span></h2><ol><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Operator Serwisu ma prawo do zmiany postanowień Regulaminu poprzez ich zamieszczenie w Serwisie i dostarczenie takiej informacji do Użytkownika na podany adres e-mail. Zmiany wchodzą w życie w terminie wskazanym przez Operatora Serwisu, chyba że obowiązujące przepisy prawa stanowią inaczej. Użytkownik powinien zapoznać się ze zmianami wprowadzonymi przez Operatora Serwisu.&nbsp;</span></li><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Użytkownik, który nie wyraża zgody na zmiany w Regulaminie, nie może korzystać z Konta po upływie terminu, o którym mowa w punkcie 11a Dalsze korzystanie przez Użytkownika z Serwisu po wejściu w życie zmian w Regulaminie jest równoznaczne z akceptacją tych zmian.</span></li><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Osoba fizyczna zawierająca z Operatorem Serwisu umowę treści cyfrowych bezpośrednio związaną z jej działalnością gospodarczą, gdy z treści tej umowy wynika, że nie posiada ona dla tej osoby charakteru zawodowego, może w terminie 14 dni od zawarcia umowy odstąpić od niej bez podania przyczyny. Odstąpienie w tym trybie nie jest możliwe, jeżeli przed upływem tego terminu:</span></li></ol><p class=\"ql-align-justify\"><span style=\"color: rgb(0, 0, 0);\">a. Operator Serwisu w pełni wykonał daną odpłatną usługę; albo</span></p><p class=\"ql-align-justify\"><span style=\"color: rgb(0, 0, 0);\">b. </span><span style=\"background-color: transparent; color: rgb(0, 0, 0);\">w przypadku usług odpłatnych, których przedmiotem jest dostarczenie usług cyfrowych Użytkownik jest zobowiązany do zapłaty ceny, Operator Serwisu rozpocznie świadczenie takich usług; oraz Użytkownik </span> <span style=\"color: rgb(0, 0, 0);\">wyraził uprzednio wolę skorzystania z usługi, będąc świadomym, że</span><span style=\"background-color: transparent; color: rgb(0, 0, 0);\"> po spełnieniu świadczenia przez Operatora Serwisu utraci prawo odstąpienia od umowy.</span></p><ol><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Operator Serwisu zastrzega sobie prawo do wiążącej interpretacji treści Regulaminu.</span></li><li class=\"ql-align-justify\"><span style=\"background-color: transparent;\">Jeżeli jakiekolwiek postanowienie niniejszego Regulaminu jest nieważne lub nieskuteczne, nie wpływa to na ważność pozostałych postanowień. Nieważne lub nieskuteczne postanowienia zostaną zastąpione postanowieniami, które w największym stopniu odpowiadają celowi biznesowemu i charakterowi relacji między Operatorem Serwisu a Użytkownikiem.</span></li></ol><h2 class=\"ql-align-right\"><br></h2>"
                });
                // HACK: Add initial homepage's contents
            }
            return app;
        }
    }
}
