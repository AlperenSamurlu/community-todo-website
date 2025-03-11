----------

#### Web Site Adresi : TaskTogether.com.tr

**Proje: Community To-Do**
# 1. Proje Tanımı

```
Community To-Do, topluluk temelli iş birliğini ve bireysel görev yönetimini
destekleyen bir platformdur. Kullanıcılar, topluluklar oluşturabilir, topluluklara
katılabilir ve topluluklar içinde veya bireysel olarak görevler organize edebilir.
```

```
Community To-Do platformunda, MSSQL veritabanı ilişkisel bir yapıda
tasarlanmış, tablolar arasında güçlü ilişkiler kurulmuş ve API işlemlerinde JSON
formatı kullanılmıştır. Proje, .NET Core MVC mimarisine uygun olarak geliştirilmiş,
modern bir görünüm için Bootstrap 5 kullanılarak responsive ve estetik bir ön yüz
tasarımı sağlanmıştır.
```

# 2. Kullanıcı Arayüzü

## 2.1 Anasayfa(Index.html)

-   Kullanıcıya giriş yapma veya kayıt olma seçenekleri sunar.

## 2.2 Kayıt Olma Sayfası(signup.html)

-   Yeni kullanıcıların kayıt olmalarını sağlar.
-   Kullanıcıdan ad, e-posta ve şifre bilgileri alır.
-   Hata ve başarı durumları için mesaj gösterir.
-   Community To-Do platformuna üye olan kullanıcılar, kayıt  
    işlemi sırasında platformun **Kullanıcı Sözleşmesi** ve  
    **Gizlilik Politikası** şartlarını kabul etmelidir.

## 2.3 Doğrulama Sayfası (verification.html)

-   Kayıt sırasında e-posta doğrulaması için kullanılır.
-   Kullanıcının e-posta adresine gönderilen kod doğrulanır.
-   Doğrulama 2 dakika içinde yapılmazsa sql tablolarından  
    kullanıcıya ait veriler silinir.

## 2.4 Giriş Sayfası (login.html)

-   Kayıtlı kullanıcıların giriş yapmasına olanak tanır.
-   Kullanıcı e-posta ve şifresi doğrulanır ve giriş yapılır.
-   Başarısız giriş durumunda hata mesajı gösterir

## 2.5 Dashboard (dashboard.html)


### 2.5.1. Topluluk İşlemleri


 ### 2.5.1.1 Topluluk Oluşturma

-   Kullanıcılar, yeni bir topluluk oluşturabilir.
-   Topluluğa bir ad, açıklama ve şifre atanır.
-   Kullanıcı, topluluğu oluşturduğunda otomatik olarak "Admin"  
    rolüne sahip olur.
-   Oluşturulan topluluk anında topluluk listesine eklenir.  

### 2.5.1.2 Topluluk Arama
    
-   Kullanıcılar, var olan toplulukları topluluk adlarına göre  
    arayabilir.
-   Arama sonuçları, topluluk adı ve açıklamasıyla birlikte dinamik  
    olarak görüntülenir.
-   Kullanıcı, ilgilendiği topluluğa katılmak için ilgili butonu  
    kullanabilir.  
    
   ### 2.5.1.3 Var Olan Bir Topluluğa Katılma
    
-   Kullanıcı, katılmak istediği topluluğu seçip, şifre ile doğrulama  
    yaparak topluluğa katılabilir.
-   Katılım başarılı olduğunda, kullanıcı topluluk üyeleri arasında "Üye"  
    olarak eklenir.
-   Şifre hatalıysa, kullanıcıya hata mesajı gösterilir.


### 2.5.1.4 Topluluktan Ayrılma


-   Kullanıcı, üyesi olduğu bir topluluktan ayrılabilir.
-   Ayrılma işlemi, kullanıcıya onay sorusu ile doğrulanır.
-   Ayrıldıktan sonra kullanıcı, topluluk üyelerinden kaldırılır ve  
    topluluğa erişimi sonlandırılır ve topluluğa ait görevlere  
    erişemez ve topluluğa görev ekleyemez.
-   Ayrılma işlemi tamamlandığında kullanıcıya bildirim yapılır  

  ### 2.5.1.5 Topluluğu Yönetme
  

-   Yalnızca topluluk admini, topluluğu yönetme yetkisine sahiptir.
-   Admin, topluluğa üye olan kullanıcıların bir listesini görebilir.
-   Admin, topluluk kurallarını ihlal eden veya gereksiz hale gelen üyeleri topluluktan çıkarabilir.
-  Admin, topluluk üyelerini terfi ettirebilir.

### 2.5.2. Görev Yönetimi


### 2.5.2.1 Yeni Görev Ekleme


-   Kullanıcı, topluluğa özel veya bireysel görevler ekleyebilir.
    
-   Görev oluşturulurken şu bilgiler girilir:  
    o Görev başlığı  
    o Görev açıklaması  
    o Başlangıç ve bitiş tarihleri  
    o Görev türü (bireysel veya topluluk temelli)
    
-   Yeni görev ekleme işlemi sonrası görevler listesine dinamik  
    olarak eklenir. 
     
   ### 2.5.2.2 Görev Güncelleme
    
-   Kullanıcılar, mevcut görevlerin detaylarını güncelleyebilir.
    
-   Güncellenen bilgiler, anında görev listesinde yenilenir.  

   ### 2.5.2.3 Görev Silme
    
-   Kullanıcı, gerek bireysel gerek topluluk bazlı görevleri silebilir.
    
-   Silinen görevler otomatik olarak listeden kaldırılır ve bildirim  
    sistemi üzerinden bilgi verilir.  
    
   ### 2.5.2.4 Görevi Tamamlama
    
-   Görev tamamlandığında, kullanıcı "Tamamlandı" durumunu  
    işaretleyebilir.
    
-   Tamamlanan görevler işaretlenerek listede ayrı bir şekilde  
    görüntülenir  
    
   ### 2.5.2.5 Bildirim Ekleme
    
-   Yeni görev ekleme, görev silme veya görev güncelleme işlemleri  
    yapıldığında, Dashboard'daki **Bildirimler** bölümüne otomatik  
    olarak bir bildirim eklenir.
    
-   Bildirimler, kullanıcının tüm aktivitelerden haberdar olmasını sağlar  
    .
    

### 2.5.3. Ayarlar


### 2.5.3.1 Şifre Değiştirme


-   Kullanıcılar, mevcut şifrelerini yeni bir şifreyle değiştirebilir.
-   Şifre değişiminde, iki kez doğrulama yapılır. Şifreler uyuşmazsa  
    hata mesajı gösterilir.  
    
   ### 2.5.3.2 E-posta Bildirimlerini Yönetme
    
-   Kullanıcılar, platformdan e-posta bildirimleri almayı etkinleştirebilir  
    veya devre dışı bırakabilir.
-   "E-Posta Bildirimlerini Etkinleştir" ve "E-Posta Bildirimlerini Devre  
    Dışı Bırak" butonlarıyla yönetim sağlanır.  
    
   

## 2.6 Sıfırlama Kodu Giriş Sayfası (password- reset-code.html)

-   Kullanıcıdan e-posta adresi alır ve şifre sıfırlama kodu gönderir

## 2.7 Sıfırlama Kodu Giriş Sayfası (password-reset-code.html)

-   Kullanıcıdan gelen doğrulama kodunu kontrol eder

## 2.8 Şifre Oluşturma Sayfası (password-reset-form.html)

-   Kullanıcının yeni bir şifre oluşturmasını sağlar

# 3. Backend

```
Community To-Do platformunun backend yapısı, ASP.NET Core framework'ü
kullanılarak geliştirilmiştir. Model-View-Controller (MVC) mimarisi temel
alınarak esnek ve sürdürülebilir bir yapı oluşturulmuştur.
```
## 3.1 Controllers (Denetleyiciler)
### 3.1.1 CommunitiesController


-   **Amaç** : Topluluklarla ilgili işlemleri yönetir.
-   **Özellikler** :  
    o Topluluk oluşturma, silme, güncelleme işlemleri.  
    o Topluluk üyelik kontrolü ve şifre doğrulama.  
    o Topluluk arama ve listeleme işlemleri.
-   **Önemli Metotlar** :  
    o GetCommunities: Mevcut toplulukları döner.  
    o CreateCommunity: Yeni topluluk oluşturur. Admin yetkisi otomatik  
    atanır.  
    o JoinCommunity: Kullanıcının bir topluluğa katılmasını sağlar. Şifre  
    kontrolü yapılır.  
    o SearchCommunities: Topluluk adında arama yapar.


### 3.1.2 TasksController


-   **Amaç** : Görev yönetimini sağlar.
    
-   **Özellikler** :  
    o Görev ekleme, silme, güncelleme ve tamamlama işlemleri.  
    o Görevlerin topluluk veya bireysel olup olmadığını belirler.
    
-   **Önemli Metotlar** :  
    o GetTasks: Tüm görevleri listeleyen API.  
    o CreateTask: Yeni görev oluşturur.  
    o UpdateTask: Mevcut bir görevin detaylarını günceller.  
    o MarkTaskAsCompleted: Görev durumunu "tamamlandı" olarak  
    işaretler.
    

### 3.1.3 UserCommunitiesController

-   **Amaç** : Kullanıcı ve topluluk arasındaki ilişkileri yönetir.
-   **Özellikler** :  
    o Kullanıcının katıldığı toplulukları listeleme.  
    o Kullanıcıyı topluluğa ekleme, çıkarma.  
    o Kullanıcının rolünü (admin, üye) değiştirme.
-   **Önemli Metotlar** :  
    o GetUserCommunities: Kullanıcının üye olduğu toplulukları döner.  
    o RemoveUserFromCommunity: Bir kullanıcıyı topluluktan çıkarır.  
    o UpdateUserRole: Kullanıcının topluluktaki rolünü günceller (ör. üye →  
    admin).

### 3.1.4 UsersController

-   **Amaç** : Kullanıcı hesaplarını ve oturum yönetimini sağlar.
-   **Özellikler** :  
    o Kullanıcı kaydı, giriş, e-posta doğrulama.  
    o Kullanıcı şifre ve e-posta bilgisi güncelleme.
-   **Önemli Metotlar** :  
    o Register: Yeni kullanıcı kaydeder ve doğrulama kodu gönderir.  
    o Login: Kullanıcının e-posta ve şifre doğrulaması yaparak oturum açar.  
    o VerifyEmail: Kullanıcının e-posta doğrulama kodunu kontrol eder.

### 3.1.5 NotificationController

-   **Amaç** : Kullanıcıya sistem olaylarıyla ilgili bildirim gönderir (ör. görev eklendi,  
    görev silindi).
-   **Özellikler** :  
    o Kullanıcılara topluluk ve görev değişiklikleri hakkında bilgi verir.
-   **Önemli Metotlar** :  
    o SendNotification: Bildirim gönderir.  
    o GetNotifications: Kullanıcının tüm bildirimlerini listeler.

## 3.2 AppDbContext

-   **Amaç** : Veritabanı ile uygulama arasındaki bağlantıyı yönetir.
-   **Teknik Detaylar** :

```
o Entity Framework Core kullanılarak ORM yapılmıştır.
o Topluluklar, kullanıcılar, görevler ,bildirimler ve kullanıcı-topluluk
ilişkileri için tablolar tanımlanmıştır.
```


## 3.3 DTO Yapısı (Data Transfer Objects)


-   **Amaç** : API üzerinden gönderilen veya alınan verileri taşır.
-   **Kullanım Amacı** :  
    o Veri aktarımını optimize etmek.  
    o Gereksiz veri döngüsünü engellemek.
-   **Önemli DTO'lar** :  
    o JoinCommunityRequest: Kullanıcının topluluğa katılmak için gerekli  
    bilgileri taşır (ör. kullanıcı ID, topluluk şifresi).  
    o UserCommunityDTO: Kullanıcı ve topluluk ilişkisini temsil eder (ör.  
    topluluk adı, rol).  
    o LoginRequest: Kullanıcının giriş bilgilerini taşır (ör. e-posta, şifre).


## 3.4 MVC Yapısına Uygunluk


-   **Model-View-Controller** mimarisi, temiz kod ve sürdürülebilirlik sağlar.  
    o **Model** : Veritabanı tabloları ve DTO'lar.  
    o **Controller** : İstekleri işleyen ve modele bağlayan kontrolcüler.  
    o **View** : Ön uçta kullanıcıya sunulan sayfalar.


## 3.5 EmailService


-   **Amaç** : Kullanıcılara doğrulama kodları ve bildirimler göndermek.
-   **Özellikler** :  
    o SMTP protokolü ile çalışır.  
    o Yeni kullanıcı kaydı sırasında e-posta doğrulama kodu gönderir.  
    o Şifre sıfırlamak için e-posta doğrulama kodu gönderir.

# 4. Veritabanı


## 4.1 __EFMigrationsHistory Tablosu


-   **Amaç** : Migration geçmişini izlemek için kullanılır.
    
-   **İçerik** :  
    o MigrationId: Her migration için benzersiz bir kimlik.  
    o ProductVersion: Migration'ın oluşturulduğu Entity Framework Core  
    sürümü.
    
-   **Önemi** :
    

```
o Veritabanı şemasında yapılan değişikliklerin kaydını tutar.
o Geriye dönük değişikliklere olanak sağlar.
```

## 4.2 Communities Tablosu

-   **Amaç** : Toplulukların detaylarını saklar.
-   **Kolonlar** :

```
o CommunityId (Primary Key): Topluluk için benzersiz bir kimlik.
o CommunityName: Topluluk adı.
o Description: Topluluk açıklaması.
o Password: Topluluğa katılım için gereken şifre.
o CreatedBy: Topluluğu oluşturan kullanıcı ID'si (Foreign Key).
```

-   **İlişkiler** :

```
o CreatedBy → Users.UserId: Topluluğu oluşturan kullanıcıyı belirtir.
o Birden Çoğa İlişki : Bir kullanıcı birden fazla topluluk oluşturabilir.
```

## 4.3 Notifications Tablosu

-   **Amaç** : Kullanıcılara bildirim göndermek için kullanılır.
-   **Kolonlar** :

```
o NotificationId (Primary Key): Bildirim için benzersiz kimlik.
o UserId (Foreign Key): Bildirimin gönderildiği kullanıcı.
o Message: Bildirim mesajı.
o CreatedAt: Bildirimin oluşturulma tarihi.
```

-   **İlişkiler** :

```
o UserId → Users.UserId: Bildirimi alan kullanıcıyı belirtir.
```

## 4.4 Tasks Tablosu

-   **Amaç** : Kullanıcılar ve topluluklar için görevleri yönetir.
    
-   **Kolonlar** :
    

```
o TaskId (Primary Key): Görev için benzersiz kimlik.
o TaskTitle: Görev başlığı.
o TaskDescription: Görev açıklaması.
o StartDate: Görevin başlangıç tarihi.
o EndDate: Görevin bitiş tarihi.
o IsCompleted: Görevin tamamlanma durumu (boolean).
o CommunityId (Foreign Key): Görev topluluğa aitse topluluğun ID'si.
o IsIndividual: Görevin bireysel mi yoksa topluluk bazlı mı olduğunu
belirler.
o CreatedBy: Görevi kimin oluşturduğunu kaydeder.
```

-   **İlişkiler** :

```
o CommunityId → Communities.CommunityId: Görevin ait olduğu
topluluğu belirtir.
o Birden Çoğa İlişki : Bir topluluk birden fazla görev içerebilir.
```

## 4.5 UserCommunities Tablosu

-   **Amaç** : Kullanıcı ve topluluk ilişkilerini yönetir.
-   **Kolonlar** :

```
o UserId (Foreign Key): Kullanıcı kimliği.
o CommunityId (Foreign Key): Topluluk kimliği.
o Role: Kullanıcının topluluk içindeki rolü (ör. Admin, Member).
o JoinedAt: Kullanıcının topluluğa katıldığı tarih.
```

-   **İlişkiler** :

```
o Birden Çoğa İlişki :
▪ UserId → Users.UserId: Kullanıcı bilgisi.
▪ CommunityId → Communities.CommunityId: Topluluk bilgisi.
```

## 4.6 Users Tablosu

-   **Amaç** : Platforma kayıtlı kullanıcıların bilgilerini tutar.
-   **Kolonlar** :

```
o UserId (Primary Key): Kullanıcı kimliği.
o UserName: Kullanıcı adı.
o Email: Kullanıcı e-posta adresi.
o PasswordHash: Kullanıcının şifresi.
o IsVerified: Kullanıcının e-posta doğrulama durumu.
o VerificationCode: E-posta doğrulama kodu.
o CreatedAt: Kullanıcının kayıt tarihi.
o IsEmailNotificationEnabled: Kullanıcının e-posta bilgilendirmesi
isteme durumu.
o ResetCode: Şifre sıfırlama kodu.
o ResetCodeExpiration: Kullanıcılara gönderilen şifre sıfırlama kodlarının
belirli bir süre sonra geçerliliğini kaybetmesini sağlar
```

-   **İlişkiler** :

```
o Kullanıcılar topluluklar oluşturabilir (CreatedBy ilişkisi).
o Kullanıcılar topluluklara katılabilir (UserCommunities ile ilişki).
```

**Tablo İlişkileri**

**1. Topluluk ve Kullanıcı İlişkisi**
-   **Birden Çoğa** : Bir kullanıcı birden fazla topluluk oluşturabilir.
-   **Çoktan Çoğa** : Kullanıcı ve topluluk ilişkileri UserCommunities tablosu ile  
    yönetilir.  
    
   **2. Topluluk ve Görev İlişkisi**
-   **Birden Çoğa** : Bir topluluk birden fazla görev içerebilir.  

  **3. Kullanıcı ve Bildirim İlişkisi**
-   **Birden Çoğa** : Bir kullanıcı birden fazla bildirim alabilir.
