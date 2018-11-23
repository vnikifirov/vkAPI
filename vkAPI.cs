///////////////////////////////////////////////////////////////////
// 
// vkAPI: Class for API VKontakte (http://vk.com/dev)
//
// Author:
//   Aleksandr Bushlanov (alex@bushlanov.pro)
//
// Copyright © 2015 All Rights Reserved.
//
// For the full copyright and license information, please view 
// the LICENSE file that was distributed with this source code.
//
///////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace vkapi
{
    #region Глобальные перечисления
    public enum ScopeList
    {
        /// <summary>
        /// Пользователь разрешил отправлять ему уведомления. 
        /// </summary>
        notify = 1,

        /// <summary>
        /// Доступ к друзьям.
        /// </summary>
        friends = 2,

        /// <summary>
        /// Доступ к фотографиям. 
        /// </summary>
        photos = 4,

        /// <summary>
        /// Доступ к аудиозаписям. 
        /// </summary>
        audio = 8,

        /// <summary>
        /// Доступ к видеозаписям. 
        /// </summary>
        video = 16,

        /// <summary>
        /// Доступ к wiki-страницам. 
        /// </summary>
        pages = 128,

        /// <summary>
        /// Добавление ссылки на приложение в меню слева.
        /// </summary>
        link = 256,

        /// <summary>
        /// Доступ к статусу пользователя.
        /// </summary>
        status = 1024,

        /// <summary>
        /// Доступ заметкам пользователя. 
        /// </summary>
        notes = 2048,

        /// <summary>
        /// (для Standalone-приложений) Доступ к расширенным методам работы с сообщениями. 
        /// </summary>
        messages = 4096,

        /// <summary>
        /// Доступ к обычным и расширенным методам работы со стеной. 
        /// </summary>
        wall = 8192,

        /// <summary>
        /// Доступ к документам пользователя.
        /// </summary>
        docs = 131072,

        /// <summary>
        /// Доступ к группам пользователя.
        /// </summary>
        groups = 262144,

        /// <summary>
        /// Доступ к email пользователя.
        /// </summary>
        email = 4194304,

        /// <summary>
        /// Доступ к оповещениям об ответах пользователю.
        /// </summary>
        notifications = 524288,

        /// <summary>
        /// Доступ к статистике групп и приложений пользователя, администратором которых он является.
        /// </summary>
        stats = 1048576,

        /// <summary>
        /// Доступ к расширенным методам работы с рекламным API.
        /// </summary>
        ads = 32768,

        /// <summary>
        /// Доступ к API в любое время со стороннего сервера (при использовании этой опции параметр expires_in, возвращаемый вместе с access_token, содержит 0 — токен бессрочный).
        /// </summary>
        offline = 65536
    }
    #endregion

    /// <remarks>
    /// Профиль пользователя.
    /// </remarks>
    public class User
    {
        /// <summary>
        /// ID пользователя.
        /// </summary>
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string deactivated { get; set; }

        public string photo_id { get; set; }
        public int verified { get; set; }
        public int blacklisted { get; set; }

        /// <summary>
        /// Пол пользователя.
        /// </summary>
        public int sex { get; set; }
        /// <summary>
        /// День рождения пользователя.
        /// </summary>
        public string bdate { get; set; }
        public Dictionary<string, string> city { get; set; }

        public string home_town { get; set; }

        public string photo_50 { get; set; }
        public string photo_100 { get; set; }
        public string photo_200_orig { get; set; }
        public string photo_200 { get; set; }
        public string photo_400_orig { get; set; }
        public string photo_max { get; set; }
        public string photo_max_orig { get; set; }

        /// <summary>
        /// В сети пользователь или нет
        /// </summary>
        public int online { get; set; }
        public string lists { get; set; }
        public string domain { get; set; }
        public int has_mobile { get; set; }

        public Dictionary<string, string> contacts { get; set; }

        /// <summary>
        /// Web сайт.
        /// </summary>
        public string site { get; set; }

        public Dictionary<string, string> education { get; set; }

        //public string last_seen { get; set; }

        public int followers_count { get; set; }
        public int common_count { get; set; }

        public Dictionary<string, int> counters { get; set; }

        /// <summary>
        /// Ник, отчествопользователя.
        /// </summary>
        public string nickname { get; set; }

        public int relation { get; set; }

        /// <summary>
        /// Временная зона пользователя.
        /// </summary>
        public int timezone { get; set; }
        public string screen_name { get; set; }
        public string maiden_name { get; set; }
        public int is_friend { get; set; }
        public int friend_status { get; set; }

    }

    class Command
    {
        public string name { get; set; }
        public NameValueCollection qs { get; set; }

        public Command (string Name, NameValueCollection Qs)
        {
            this.name = Name;
            this.qs = Qs;
        }
    }

    /// <remarks>
    /// Сервер для загрузки изображений на стену пользователя
    /// </remarks>
    public class PhotoUploadServer
    {
        /// <summary>
        /// Адрес для загрузки изображения в альбом стены
        /// </summary>
        public string upload_url { get; set; }
        /// <summary>
        /// ID альбома в который загруженно изображение.
        /// </summary>
        public int album_id { get; set; }
        /// <summary>
        /// ID пользователя которому принадлежит изображение.
        /// </summary>
        public int user_id { get; set; }
    }

    /// <remarks>
    /// Сервер для загрузки аудио на стену пользователя
    /// </remarks>
    public class AudioUploadServer
    {
        /// <summary>
        /// Адрес для загрузки аудио
        /// </summary>
        public string upload_url { get; set; }
    }

    /// <remarks>
    /// Объект photo, описывающий фотографию.
    /// </remarks>
    public class Photo
    {
        /// <summary>
        /// ID фотографии.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Идентификатор альбома, в котором находится фотография.
        /// </summary>
        public int album_id { get; set; }

        /// <summary>
        /// Идентификатор владельца фотографии.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Идентификатор пользователя, загрузившего фото (если фотография размещена в сообществе). 
        /// Для фотографий, размещенных от имени сообщества, user_id=100. 
        /// </summary>
        public int user_id { get; set; }

        public string photo_75 { get; set; }
        public string photo_130 { get; set; }
        public string photo_604 { get; set; }
        public string photo_807 { get; set; }
        public string photo_1280 { get; set; }
        public string photo_2560 { get; set; }

        /// <summary>
        /// Ширина оригинала фотографии в пикселах. 
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// Высота оригинала фотографии в пикселах.
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// Описание фотографии.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Дата добавления в формате unixtime. 
        /// </summary>
        public int date { get; set; }
    }

    /// <remarks>
    /// Объект audio, описывающий аудиозапись.
    /// </remarks>
    public class Audio
    {
        /// <summary>
        /// Идентификатор аудиозаписи.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Идентификатор владельца аудиозаписи.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public string artist { get; set; }

        /// <summary>
        /// Название композиции.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Длительность аудиозаписи в секундах.
        /// </summary>
        public int duration { get; set; }

        /// <summary>
        /// Ссылка на mp3.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Идентификатор альбома, в котором находится аудиозапись (если присвоен).
        /// </summary>
        public int album_id { get; set; }

    }

    /// <remarks>
    /// Обьект фотоальбома
    /// </remarks>
    public class PhotoAlbum
    {
        /// <summary>
        /// Идентификатор альбома
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Идентификатор фотографии, которая является обложкой (0, если обложка отсутствует)
        /// </summary>
        public int thumb_id { get; set; }
        /// <summary>
        /// Идентификатор владельца альбома
        /// </summary>
        public int owner_id { get; set; }
        /// <summary>
        /// Название альбома
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Описание альбома; (не приходит для системных альбомов)
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Дата создания альбома в формате unixtime; (не приходит для системных альбомов)
        /// </summary>
        public int created { get; set; }
        /// <summary>
        /// Дата последнего обновления альбома в формате unixtime; (не приходит для системных альбомов)
        /// </summary>
        public int updated { get; set; }
        /// <summary>
        /// Количество фотографий в альбоме
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// Ccылка на изображение обложки альбома (если был указан параметр need_covers)
        /// </summary>
        public string thumb_src { get; set; }
        /// <summary>
        /// Использовать посленее фото в качестве обложки
        /// </summary>
        public int thumb_is_last { get; set; }
        //public int privacy_view { get; set; }
        //public int privacy_comment { get; set; }
    }

    /// <remarks>
    /// Объект описывающий комментарий.
    /// </remarks>
    public class Comment
    {
        /// <summary>
        /// Идентификатор комментария.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Идентификатор автора комментария.
        /// </summary>
        public int from_id { get; set; }

        /// <summary>
        /// Дата создания комментария в формате unixtime.
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// Текст комментария.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Идентификатор пользователя или сообщества, в ответ которому оставлен текущий комментарий.
        /// </summary>
        public int reply_to_user { get; set; }

        /// <summary>
        /// Идентификатор комментария, в ответ на который оставлен текущий.
        /// </summary>
        public int reply_to_comment { get; set; }
    }

    /// <remarks>
    /// Объект диалога
    /// </remarks>
    public class Dialog
    {
        /// <summary>
        /// Статус прочитанно ли собщение
        /// </summary>
        public int unread { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public Message message { get; set; }
    }

    /// <summary>
    /// Объект, описывающий личное сообщение.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Идентификатор сообщения (не возвращается для пересланных сообщений).
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Идентификатор пользователя, в диалоге с которым находится сообщение.
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// Идентификатор автора сообщения.
        /// </summary>
        public int from_id { get; set; }

        // TODO: ???
        /// <summary>
        /// 
        /// </summary>
        public User user { get; set; }

        /// <summary>
        /// Дата отправки сообщения в формате unixtime.
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// Статус сообщения (0 — не прочитано, 1 — прочитано, не возвращается для пересланных сообщений).
        /// </summary>
        public int read_state { get; set; }

        /// <summary>
        /// тип сообщения (0 — полученное, 1 — отправленное, не возвращается для пересланных сообщений).
        /// </summary>
        //public int out { get; set; }

        /// <summary>
        /// Заголовок сообщения или беседы.
        /// </summary>
        public string title  { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string body { get; set; }

        /// <summary>
        /// Массив медиа-вложений.
        /// </summary>
        ///public string attachments { get; set; }

        /// <summary>
        /// Содержатся ли в сообщении emoji-смайлы.
        /// </summary>
        public int emoji { get; set; }

        /// <summary>
        /// Является ли сообщение важным.
        /// </summary>
        public int important { get; set; }

        /// <summary>
        /// Удалено ли сообщение.
        /// </summary>
        public int deleted { get; set; }

        public Message()
        {
            this.user = new User();
        }
    }

    public class vkAPI
    {
        /// <summary>
        /// Версия используемого API
        /// </summary>
        private const string versionAPI = "5.34";

        public string accessToken { get; set; }

        public int appId { get; set; }

        public string appSecret { get; set; }

        public int scope { get; set; }

        public int userId { get; set; }

        public bool authStatus { get; set; }

        private Form authFrm { get; set; }


        public delegate string ECDelegate(string name, NameValueCollection qs, string accessToken,string method = "get");



        public vkAPI(int AppID, int Scope)
        {
            this.appId = AppID;
            this.scope = Scope;
        }


        /// <summary>
        /// Диалог авторизации
        /// </summary>
        public bool AuthDialog()
        {
            this.authFrm = new Form();
            this.authFrm.Text = "Авторизация";
            this.authFrm.TopMost = true;
            this.authFrm.Size = new System.Drawing.Size(800, 600);
            this.authFrm.StartPosition = FormStartPosition.CenterScreen;
            this.authFrm.Load += new System.EventHandler(this.authFrm_Load);
            this.authFrm.ShowDialog();
            return true;
        }


        private void authFrm_Load(object sender, EventArgs e)
        {
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.ScriptErrorsSuppressed = true;
            this.authFrm.Controls.Add(webBrowser);
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Navigate(String.Format("https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri=https://oauth.vk.com/blank.html&display=popup&v={2}&response_type=token", this.appId, this.scope, versionAPI));
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }


        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString().IndexOf("access_token") != -1)
            {
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in myReg.Matches(e.Url.ToString()))
                {
                    if (m.Groups["name"].Value == "access_token") { this.accessToken = m.Groups["value"].Value; }
                    else if (m.Groups["name"].Value == "user_id") { this.userId = Convert.ToInt32(m.Groups["value"].Value); }
                }
                if(!string.IsNullOrEmpty(this.accessToken) && this.userId != 0)
                {
                    // ключ найден, 
                    // инициализируем api вконтакте
                    this.authStatus = true;
                    this.authFrm.Close();
                }
                // авторизоваться неудалось
                else { this.authStatus = false; }
            }
            // пользователь отказался входить
            else if (e.Url.ToString().IndexOf("user_denied") != -1) {  this.authStatus = false; }
        }


        /// <summary>
        /// Возвращает профиль текущего пользователя
        /// </summary>
        public User UsersGet()
        {
            return this.UsersGet(this.userId);
        }


        /// <summary>
        /// Возвращает профиль указанного пользователя
        /// </summary>
        /// <param name="uid">Идентификатор пользователя</param>
        public User UsersGet(int uid)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_ids"] = uid.ToString();
            qs["fields"] = "uid,first_name,last_name,nickname,domain,sex,bdate,city,country,timezone,photo,has_mobile,rate,contacts,education,online,photo_50";
            JObject o = JObject.Parse(this.ExecuteCommand("users.get", qs));
            JArray array = (JArray)o["response"];
            var user = JsonConvert.DeserializeObject<User>(array.First().ToString());
            return user;
        }


        /// <summary>
        /// Отправляет пост на стену указанного пользователя
        /// </summary>
        /// <param name="uid">Идентификатор пользователя</param>
        /// <param name="friendsOnly">Кто будет видеть сообщение</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="attachment">Вложения</param>
        /// <param name="publishDate">Время отложенной публикации поста</param>
        /// <param name="geo"></param>
        public string WallPost(int uid, int friendsOnly = 0, string message = "", string attachment = "", string publishDate = "", NameValueCollection geo = null)
        {
            string attch = "";

            if (!string.IsNullOrEmpty(attachment))
            {
                string[] attachs = attachment.Split(new Char[] { ',' });

                foreach (string attach in attachs)
                {
                    if (!string.IsNullOrEmpty(attach)) {
                        string[] split = attach.Split(new Char[] { '.' });

                        if (split.Last() == "jpg" || split.Last() == "png" || split.Last() == "gif")
                        {
                            var server = this.PhotosGetWallUploadServer();
                            var img = this.UploadToURL(server.upload_url, attach.Trim());
                            var resp = this.PhotosSaveWallPhoto(img["server"].ToString(), img["photo"].ToString(), img["hash"].ToString(), geo);
                            attch += (attch != "") 
                                ? (",photo" + resp.owner_id + "_" + resp.id) 
                                : ("photo" + resp.owner_id + "_" + resp.id);
                            //attch = "photo" + resp.owner_id + "_" + resp.id;
                        }
                        else if (split.Last() == "mp3")
                        {
                            var server = this.AudioGetUploadServer();
                            var audio = this.UploadToURL(server.upload_url, attach.Trim());
                            var resp = this.AudioSave(audio["server"].ToString(), audio["audio"].ToString(), audio["hash"].ToString());
                            attch += (attch != "")
                                ? (",audio" + resp.owner_id + "_" + resp.id)
                                : ("audio" + resp.owner_id + "_" + resp.id);
                            //attch = "audio" + resp.owner_id + "_" + resp.id;
                        }
                        else if (split.Last() == "mp4" || split.Last() == "avi")
                        {
                            var video = this.VideoSave(attach.Trim());
                            attch += (attch != "")
                                ? (",video" + this.userId.ToString() + "_" + video["video_id"].ToString())
                                : ("video" + this.userId.ToString() + "_" + video["video_id"].ToString());
                        }
                        else
                        {
                            attch += (attch != "")
                                ? ("," + attach)
                                : (attach);
                        }
                    }
                }
            }

            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = uid.ToString();
            qs["friends_only"] = friendsOnly.ToString();
            if(geo != null)
            {
                qs["lat"] = geo["lat"];
                qs["long"] = geo["long"];
            }
            if (message != string.Empty) qs["message"] = message;
            if (attch != string.Empty) qs["attachment"] = attch;
            //qs["attachment"] = @"photo4035208_371353650";
            if (publishDate != string.Empty)
            {
                long publish_date_time = ((DateTime.Parse(publishDate)).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                qs["publish_date"] = publish_date_time.ToString();
            }

            JObject o = JObject.Parse(this.ExecuteCommand("wall.post", qs, "post"));
            return o["response"]["post_id"].ToString();
        }


        /// <summary>
        /// Публикует или предлогает новость на стене группы
        /// </summary>
        /// <param name="owner_id">Идентификатор сообщества со знаком минус</param>
        /// <param name="from_group">Данный параметр учитывается, если owner_id < 0 (запись публикуется на стене группы). 1 — запись будет опубликована от имени группы, 0 — запись будет опубликована от имени пользователя (по умолчанию). </param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="attachment">Вложения</param>
        /// <param name="publishDate">Время отложенной публикации поста</param>
        /// <returns></returns>
        public string WallPostToPublic(int owner_id, int from_group = 0, string message = "", string attachment = "", string publishDate = "")
        {
            string attch = "";

            if (!string.IsNullOrEmpty(attachment))
            {
                string[] attachs = attachment.Split(new Char[] { ',' });

                foreach (string attach in attachs)
                {
                    if (!string.IsNullOrEmpty(attach))
                    {
                        string[] split = attach.Split(new Char[] { '.' });

                        if (split.Last() == "jpg" || split.Last() == "png" || split.Last() == "gif")
                        {
                            var server = this.PhotosGetWallUploadServer();
                            var img = this.UploadToURL(server.upload_url, attach.Trim());
                            var resp = this.PhotosSaveWallPhoto(img["server"].ToString(), img["photo"].ToString(), img["hash"].ToString());
                            attch += (attch != "") ? (",photo" + resp.owner_id + "_" + resp.id) : ("photo" + resp.owner_id + "_" + resp.id);
                        }
                        else if (split.Last() == "mp3")
                        {
                            var server = this.AudioGetUploadServer();
                            var audio = this.UploadToURL(server.upload_url, attach.Trim());
                            var resp = this.AudioSave(audio["server"].ToString(), audio["audio"].ToString(), audio["hash"].ToString());
                            attch += (attch != "") ? (",audio" + resp.owner_id + "_" + resp.id) : ("audio" + resp.owner_id + "_" + resp.id);
                        }
                    }
                }
            }

            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["from_group"] = from_group.ToString();

            if (message != string.Empty) qs["message"] = message;
            if (attch != string.Empty) qs["attachment"] = attch;

            if (publishDate != string.Empty)
            {
                long publish_date_time = ((DateTime.Parse(publishDate)).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                qs["publish_date"] = publish_date_time.ToString();
            }

            JObject o = JObject.Parse(this.ExecuteCommand("wall.post", qs, "post"));
            return o["response"]["post_id"].ToString();
        }


        /// <summary>
        /// Возвращает адрес сервера для загрузки фотографии на стену пользователя или сообщества.
        /// </summary>
        /// <param name="group_id">Идентификатор сообщества, на стену которого нужно загрузить фото (без знака «минус»). </param>
        public PhotoUploadServer PhotosGetWallUploadServer(int group_id = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            if (group_id != 0) qs["group_id"] = group_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.getWallUploadServer", qs, "post"));
            //JArray array = (JArray)o["response"];
            var server = JsonConvert.DeserializeObject<PhotoUploadServer>(o["response"].ToString());
            return server;
        }


        /// <summary>
        /// Возвращает адрес сервера (необходимый для загрузки) и данные видеозаписи.
        /// </summary>
        /// <param name="name">Название видеофайла.</param>
        /// <param name="description">Описание видеофайла.</param>
        /// <returns></returns>
        public JObject VideoSave(string file, string name = "", string description = "")
        {
            NameValueCollection qs = new NameValueCollection();
            if (name != "") qs["name"] = name.ToString();
            if (description != "") qs["description"] = description.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("video.save", qs, "post"));
            var server = o["response"]["upload_url"].ToString();
            var resp = this.UploadToURL(server, file);
            return resp;
        }


        /// <summary>
        /// Возвращает адрес сервера для загрузки музыки.
        /// </summary>
        public AudioUploadServer AudioGetUploadServer()
        {
            NameValueCollection qs = new NameValueCollection();
            JObject o = JObject.Parse(this.ExecuteCommand("audio.getUploadServer", qs, "post"));
            //JArray array = (JArray)o["response"];
            var server = JsonConvert.DeserializeObject<AudioUploadServer>(o["response"].ToString());
            return server;
        }


        /// <summary>
        /// Загрузка музыки по полученному URL
        /// </summary>
        /// <param name="URL">Адрес для загрузки</param>
        /// <param name="file_path">Путь до файла</param>
        public JObject AudioUploadToURL(string URL, string file_path)
        {
            WebClient myWebClient = new WebClient();
            byte[] responseArray = myWebClient.UploadFile(URL, file_path);
            var json = JObject.Parse(System.Text.Encoding.ASCII.GetString(responseArray));
            return json;
        }


        /// <summary>
        /// Сохраняет предварительно загруженную аудио запись
        /// </summary>
        public Audio AudioSave(string server, string audio, string hash)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["server"] = server;
            qs["audio"] = audio;
            qs["hash"] = hash;
            JObject o = JObject.Parse(this.ExecuteCommand("audio.save", qs, "post"));
            JObject array = (JObject)o["response"];
            var newAudio = JsonConvert.DeserializeObject<Audio>(array.ToString());
            return newAudio;
        }


        /// <summary>
        /// Загрузка файла по полученному URL
        /// </summary>
        /// <param name="URL">Адрес для загрузки</param>
        /// <param name="file_path">Путь до файла</param>
        public JObject UploadToURL(string URL, string file_path)
        {
            WebClient myWebClient = new WebClient();
            byte[] responseArray = myWebClient.UploadFile(URL, file_path);
            var json = JObject.Parse(System.Text.Encoding.ASCII.GetString(responseArray));
            return json;
        }


        /// <summary>
        /// Загрузка фото по полученному URL
        /// </summary>
        /// <param name="URL">Адрес для загрузки</param>
        /// <param name="file_path">Путь до файла</param>
        public JObject PhotosUploadPhotoToUrl(string URL, string file_path)
        {
            WebClient myWebClient = new WebClient();
            byte[] responseArray = myWebClient.UploadFile(URL, file_path);
            var json = JObject.Parse(System.Text.Encoding.ASCII.GetString(responseArray));
            return json;
        }


        /// <summary>
        /// Сохраняет предварительно загруженную картинку в альбом стены.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="photo"></param>
        /// <param name="hash"></param>
        /// <param name="geo"></param>
        /// <returns></returns>
        public Photo PhotosSaveWallPhoto(string server, string photo, string hash, NameValueCollection geo = null)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["server"] = server;
            qs["photo"] = photo;
            qs["hash"] = hash;
            if (geo != null)
            {
                qs["latitude"] = geo["lat"];
                qs["longitude"] = geo["long"];
            }
            JObject o = JObject.Parse(this.ExecuteCommand("photos.saveWallPhoto", qs));
            JArray array = (JArray)o["response"];
            var newPhoto = JsonConvert.DeserializeObject<Photo>(array.First().ToString());
            return newPhoto;
        }


        // TODO: ???
        public string PhotosSaveOwnerPhoto(string photos_list, string square_crop = "")
        {
            // Получаем адрес сервера
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = this.userId.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.getOwnerPhotoUploadServer", qs));
            var server = o["response"]["upload_url"].ToString();

            //отправка файла на полученый сервер
            JObject retUploadPhoto;
            if (!string.IsNullOrWhiteSpace(square_crop))
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc["_square_crop"] = square_crop;
                retUploadPhoto = HttpUploadFile(server, photos_list, @"file1", @"image/jpeg", nvc);
            }
            else
            {
                retUploadPhoto = HttpUploadFile(server, photos_list);
            }
            
            qs["server"] = retUploadPhoto["server"].ToString();
            qs["photo"] = retUploadPhoto["photo"].ToString();
            qs["hash"] = retUploadPhoto["hash"].ToString();

            //if (!string.IsNullOrEmpty(caption)) qs["caption"] = caption;
            //if (!string.IsNullOrEmpty(latitude)) qs["latitude"] = latitude;
            //if (!string.IsNullOrEmpty(longitude)) qs["longitude"] = longitude;
            return this.ExecuteCommand("photos.saveOwnerPhoto", qs, "post");

        }


        /// <summary>
        /// Помечает текущего пользователя как offline.
        /// </summary>
        /// <returns></returns>
        public string accountSetOffline()
        {
            NameValueCollection qs = new NameValueCollection();
            JObject o = JObject.Parse(this.ExecuteCommand("account.setOffline", qs));
            return o["response"].ToString();
        }


        /// <summary>
        /// Устанавливает новый статус текущему пользователю или сообществу.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string statusSet(string text)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["text"] = text;
            JObject o = JObject.Parse(this.ExecuteCommand("status.set", qs));
            return o["response"].ToString();
        }


        /// <summary>
        /// Получает текст статуса пользователя или сообщества.
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public string statusGet(int user_id = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            if(user_id != 0) qs["user_id"] = user_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("status.get", qs));
            return o["response"]["text"].ToString();
        }


        /// <summary>
        /// Возвращает список альбомов
        /// </summary>
        /// <param name="need_system">Отоброжать системные</param>
        /// <param name="owner_id">Пользователь (по умолчанию текущий)</param>
        /// <returns></returns>
        public IList<PhotoAlbum> PhotosGetAlbums(int owner_id = 0, int need_system = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["need_covers"] = @"1";
            if (owner_id != 0) qs["owner_id"] = owner_id.ToString();
            if (need_system != 0) qs["need_system"] = need_system.ToString();
            // JObject o = JObject.Parse(this.ExecuteCommand("photos.getAlbums", qs));
            // JArray array = (JArray)o["response"]["items"];
            //List<PhotoAlbum> albums = JsonConvert.DeserializeObject<List<PhotoAlbum>>(o["response"]["items"].ToString());
            //var albums = JsonConvert.DeserializeObject<PhotoAlbum>(array.ToString());

            JObject o = JObject.Parse(this.ExecuteCommand("photos.getAlbums", qs));
            IList<JToken> items = o["response"]["items"].Children().ToList();
            IList<PhotoAlbum> results = new List<PhotoAlbum>();
            foreach(JToken item in items)
            {
                PhotoAlbum searchResult = JsonConvert.DeserializeObject<PhotoAlbum>(item.ToString());
                results.Add(searchResult);
            }
            return results;
        }


        /// <summary>
        /// Возвращает список фотографий в альбоме.
        /// </summary>
        /// <param name="owner_id">Идентификатор владельца альбома.</param>
        /// <param name="album_id">Идентификатор альбома.</param>
        /// <returns></returns>
        public IList<Photo> PhotosGet(int owner_id, int album_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["album_id"] = album_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.get", qs));
            JArray array = (JArray)o["response"]["items"];
            return JsonConvert.DeserializeObject<IList<Photo>>(array.ToString());
        }


        /// <summary>
        /// Создает пустой альбом для фотографий.
        /// </summary>
        /// <param name="title">Название альбома.</param>
        /// <param name="description">Текст описания альбома.</param>
        /// <returns></returns>
        public PhotoAlbum PhotosCreateAlbum(string title, string description = "")
        {
            NameValueCollection qs = new NameValueCollection();
            qs["title"] = title;
            if (!string.IsNullOrEmpty(description)) { qs["description"] = description; }
            JObject o = JObject.Parse(this.ExecuteCommand("photos.createAlbum", qs));
            return JsonConvert.DeserializeObject<PhotoAlbum>(o["response"].ToString());
        }


        /// <summary>
        /// Возвращает адрес сервера для загрузки фотографий.
        /// </summary>
        /// <param name="album_id">Идентификатор альбома.</param>
        /// <param name="group_id">Идентификатор альбома.</param>
        /// <returns></returns>
        public PhotoUploadServer PhotosGetUploadServer(int album_id, int group_id = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["album_id"] = album_id.ToString();
            if (group_id != 0) qs["group_id"] = group_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.getUploadServer", qs, "post"));
            //JArray array = (JArray)o["response"];
            var server = JsonConvert.DeserializeObject<PhotoUploadServer>(o["response"].ToString());
            return server;
        }


        /// <summary>
        /// Получает список коментариев к записи на стене пользователя.
        /// </summary>
        /// <param name="owner_id">ID пользователя</param>
        /// <param name="post_id">ID записи</param>
        /// <param name="need_likes">Возвращать информацию о лайках</param>
        /// <param name="count">Число комментариев, которые необходимо получить. По умолчанию — 10, максимальное значение — 100</param>
        /// <param name="offset">Сдвиг, необходимый для получения конкретной выборки результатов</param>
        public IList<Comment> WallGetComments(int owner_id, int post_id, int need_likes = 1, int count = 10, int offset = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["post_id"] = post_id.ToString();
            qs["need_likes"] = need_likes.ToString();
            qs["count"] = count.ToString();
            if (offset != 0) qs["offset"] = offset.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("wall.getComments", qs, "post"));
            JArray array = (JArray)o["response"]["items"];
            var comments = JsonConvert.DeserializeObject<IList<Comment>>(array.ToString());
            return comments;
        }


        /// <summary>
        /// Возвращает список комментариев к фотографии.
        /// </summary>
        /// <param name="owner_id">Идентификатор пользователя или сообщества, которому принадлежит фотография.</param>
        /// <param name="photo_id">Идентификатор фотографии.</param>
        /// <param name="need_likes">1 — будет возвращено дополнительное поле likes. По умолчанию поле likes не возвращается.</param>
        /// <param name="count">Количество комментариев, которое необходимо получить.</param>
        /// <param name="offset">Смещение, необходимое для выборки определенного подмножества комментариев. По умолчанию — 0. </param>
        /// <returns></returns>
        public IList<Comment> PhotosGetComments(int owner_id, int photo_id, int need_likes = 1, int count = 10, int offset = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["photo_id"] = photo_id.ToString();
            qs["need_likes"] = need_likes.ToString();
            qs["count"] = count.ToString();
            if (offset != 0) qs["offset"] = offset.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.getComments", qs, "post"));
            JArray array = (JArray)o["response"]["items"];
            var comments = JsonConvert.DeserializeObject<IList<Comment>>(array.ToString());
            return comments;
        }


        /// <summary>
        /// Добавляет комментарий к записи на стене пользователя или сообщества.
        /// </summary>
        /// <param name="owner_id">Идентификатор пользователя или сообщества, на чьей стене находится запись, к которой необходимо добавить комментарий.</param>
        /// <param name="post_id">Идентификатор записи на стене пользователя или сообщества. </param>
        /// <param name="text">Текст комментария к записи.</param>
        /// <param name="reply_to_comment">Идентификатор комментария, в ответ на который должен быть добавлен новый комментарий.</param>
        /// <param name="attachments">Список объектов, приложенных к комментарию и разделённых символом ",".</param>
        /// <param name="sticker_id">Идентификатор стикера.</param>
        /// <returns></returns>
        public string WallAddComment(int owner_id, int post_id, string text, int reply_to_comment = 0, string attachments = "", int sticker_id = 0)
        {
            string attch = "";
            if (!string.IsNullOrEmpty(attachments))
            {
                string[] attachs = attachments.Split(new Char[] { ',' });
                foreach (string attach in attachs)
                {
                    if (!string.IsNullOrEmpty(attach))
                    {
                        string[] split = attach.Split(new Char[] { '.' });
                        if (split.Last() == "jpg" || split.Last() == "png" || split.Last() == "gif")
                        {
                            var server = this.PhotosGetWallUploadServer();
                            var img = this.UploadToURL(server.upload_url, attach.Trim());
                            var resp = this.PhotosSaveWallPhoto(img["server"].ToString(), img["photo"].ToString(), img["hash"].ToString());
                            attch += (attch != "") ? (",photo" + resp.owner_id + "_" + resp.id) : ("photo" + resp.owner_id + "_" + resp.id);
                            //attch = "photo" + resp.owner_id + "_" + resp.id;
                        }
                    }
                }
            }

            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["post_id"] = post_id.ToString();
            qs["text"] = text.ToString();
            if (reply_to_comment != 0) qs["reply_to_comment"] = reply_to_comment.ToString();
            if (attch != "") qs["attachments"] = attch.ToString();
            if (sticker_id != 0) qs["sticker_id"] = sticker_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("wall.addComment", qs, "post"));
            return o["response"]["comment_id"].ToString();
        }


        /// <summary>
        /// Создает новый комментарий к фотографии.
        /// </summary>
        /// <param name="owner_id">Идентификатор пользователя или сообщества, которому принадлежит фотография.</param>
        /// <param name="photo_id">Идентификатор фотографии.</param>
        /// <param name="message">Текст комментария (является обязательным, если не задан параметр attachments).</param>
        /// <param name="reply_to_comment">Идентификатор комментария, в ответ на который нужно оставить текущий.</param>
        /// <returns></returns>
        public string PhotosCreateComment(int owner_id, int photo_id, string message, int reply_to_comment = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["photo_id"] = photo_id.ToString();
            qs["message"] = message.ToString();
            if (reply_to_comment != 0) qs["reply_to_comment"] = reply_to_comment.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("photos.createComment", qs, "post"));
            return o["response"].ToString();
        }


        /// <summary>
        /// Cоздает новый комментарий к видеозаписи
        /// </summary>
        /// <param name="owner_id">Идентификатор пользователя или сообщества, которому принадлежит видеозапись.</param>
        /// <param name="video_id">Идентификатор видеозаписи.</param>
        /// <param name="message">Текст комментария (является обязательным, если не задан параметр attachments).</param>
        /// <param name="reply_to_comment">Идентификатор комментария, в ответ на который должен быть добавлен новый комментарий.</param>
        /// <returns></returns>
        public string VideoCreateComment(int owner_id, int video_id, string message, int reply_to_comment = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = owner_id.ToString();
            qs["video_id"] = video_id.ToString();
            qs["message"] = message.ToString();
            if (reply_to_comment != 0) qs["reply_to_comment"] = reply_to_comment.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("video.createComment", qs, "post"));
            return o["response"].ToString();
        }


        /// <summary>
        /// Удаляет запись со стены.
        /// </summary>
        /// <param name="post_id">Идентификатор записи на стене.</param>
        /// <param name="owner_id">Идентификатор пользователя или сообщества, на стене которого находится запись.</param>
        /// <returns></returns>
        public string WallDelete(int post_id, int owner_id = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["post_id"] = post_id.ToString();
            if (owner_id != 0) qs["owner_id"] = owner_id.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("wall.delete", qs));
            return o["response"].ToString();
        }

        /// <summary>
        /// Регистрирует нового пользователя по номеру телефона.
        /// </summary>
        /// <param name="first_name">Имя пользователя.</param>
        /// <param name="last_name">Фамилия пользователя. </param>
        /// <param name="phone">Номер телефона регистрируемого пользователя.</param>
        /// <param name="password">Пароль пользователя, который будет использоваться при входе. Не меньше 6 символов. Также пароль может быть указан позже, при вызове метода auth.confirm.</param>
        /// <param name="sex">Пол пользователя: 1 — женский, 2 — мужской.</param>
        /// <returns></returns>
        public string AuthSignup(string first_name, string last_name, string phone, string password, int sex)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["client_id"] = this.appId.ToString();
            qs["client_secret"] = this.appSecret.ToString();
            qs["first_name"] = first_name.ToString();
            qs["last_name"] = last_name.ToString();
            qs["phone"] = phone.ToString();
            
            qs["sex"] = sex.ToString();
            qs["test_mode"] = "0";
            JObject o = JObject.Parse(this.ExecuteCommand("auth.signup", qs, "post"));
            return o["response"]["sid"].ToString();
        }


        /// <summary>
        /// Завершает регистрацию нового пользователя, начатую методом auth.signup, по коду, полученному через SMS.
        /// </summary>
        /// <param name="phone">Номер телефона регистрируемого пользователя.</param>
        /// <param name="code">Код, отправленный пользователю по SMS.</param>
        /// <param name="passsword">пароль пользователя, который будет использоваться при входе. (Следует передавать только если он не был передан на предыдущем шаге auth.signup) Не меньше 6 символов.</param>
        /// <returns></returns>
        public string AuthConfirm(string phone, string code, string password = "")
        {
            NameValueCollection qs = new NameValueCollection();
            qs["client_id"] = this.appId.ToString();
            qs["client_secret"] = this.appSecret.ToString();
            qs["phone"] = phone;
            qs["code"] = code;
            if (password != "") qs["password"] = password;
            qs["test_mode"] = "0";
            JObject o = JObject.Parse(this.ExecuteCommand("auth.confirm", qs, "post"));
            return o.ToString();
        }


        /// <summary>
        /// Возвращает список диалогов текущего пользователя.
        /// </summary>
        /// <param name="offset">Смещение, необходимое для выборки определенного подмножества сообщений.</param>
        /// <param name="count">Количество диалогов, которое необходимо получить.</param>
        /// <param name="preview_length">Количество символов, по которому нужно обрезать сообщение. Укажите 0, если Вы не хотите обрезать сообщение. (по умолчанию сообщения не обрезаются).</param>
        /// <param name="unread">Значение 1 означает, что нужно вернуть только диалоги в которых есть непрочитанные входящие сообщения. По умолчанию 0.</param>
        /// <returns></returns>
        public IList<Dialog> MessagesGetDialogs(int offset = 0, int count = 20, int preview_length = 0, int unread = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["offset"] = offset.ToString();
            qs["count"] = count.ToString();
            qs["preview_length"] = preview_length.ToString();
            qs["unread"] = unread.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("messages.getDialogs", qs, "post"));
            JArray array = (JArray)o["response"]["items"];
            var dialogs = JsonConvert.DeserializeObject<IList<Dialog>>(array.ToString());
            string users = "";
            foreach (var dialog in dialogs)
            {
                users += dialog.message.user_id.ToString() + ", ";
            }

            qs.Clear();
            qs["user_ids"] = users;
            qs["fields"] = @"photo_50";

            o = JObject.Parse(this.ExecuteCommand("users.get", qs));
            array = (JArray)o["response"];

            for (int i = 0; i < dialogs.Count; i++)
            {
                foreach (var item in array)
                {
                    if (dialogs[i].message.user_id == int.Parse(item["id"].ToString()))
                    {
                        dialogs[i].message.user.first_name = item["first_name"].ToString();
                        dialogs[i].message.user.last_name = item["last_name"].ToString();
                        dialogs[i].message.user.photo_50 = item["photo_50"].ToString();
                    }
                }
            }

            return dialogs;
        }


        /// <summary>
        /// Возвращает историю сообщений для указанного пользователя.
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя, историю переписки с которым необходимо вернуть.</param>
        /// <param name="offset">Смещение, необходимое для выборки определенного подмножества сообщений, должен быть >= 0, если не передан параметр start_message_id, и должен быть <= 0, если передан.</param>
        /// <param name="count">Количество сообщений, которое необходимо получить (но не более 200).</param>
        /// <returns></returns>
        public IList<Message> MessageGetHistory(int user_id, int offset = 0, int count = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_id"] = user_id.ToString();
            if (offset != 0) qs["offset"] = offset.ToString();
            if (count != 0) qs["count"] = count.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("messages.getHistory", qs, "post"));
            JArray array = (JArray)o["response"]["items"];
            //Debug.WriteLine(o["response"]["items"].ToString());
            var messages = JsonConvert.DeserializeObject<IList<Message>>(array.ToString());
            return messages;
        }


        /// <summary>
        /// Отправка сообщения пользователю
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя</param>
        /// <param name="message">Текст сообщения</param>
        /// <returns></returns>
        public string MessagesSend(int user_id, string message)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_id"] = user_id.ToString();
            qs["message"] = message;
            return this.ExecuteCommand("messages.send", qs, "post");
        }

        /// <summary>
        /// Удаляет все личные сообщения в диалоге.
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя. Если требуется очистить историю беседы, используйте chat_id.</param>
        /// <param name="offset">Начиная с какого сообщения нужно удалить переписку. (По умолчанию удаляются все сообщения начиная с первого).</param>
        /// <param name="count">Сколько сообщений нужно удалить. Обратите внимание, что на метод наложено ограничение, за один вызов нельзя удалить больше 10000 сообщений, поэтому если сообщений в переписке больше — метод нужно вызывать несколько раз.</param>
        /// <returns></returns>
        public string MessagesDeleteDialog(int user_id, int offset = 0, int count = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_id"] = user_id.ToString();
            if (offset != 0) qs["offset"] = offset.ToString();
            if (count != 0) qs["count"] = count.ToString();
            JObject o = JObject.Parse(this.ExecuteCommand("messages.deleteDialog", qs));
            return o["response"].ToString();
        }


        /// <summary>
        /// Возвращает текущее время на сервере ВКонтакте в unixtime.
        /// </summary>
        /// <returns>Массив дат: [0]timestamp, [1]date string</returns>
        public string[] UtilsGetServerTime()
        {
            NameValueCollection qs = new NameValueCollection();
            JObject o = JObject.Parse(this.ExecuteCommand("utils.getServerTime", qs));
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((double)o["response"]).ToLocalTime();
            return new string[] { o["response"].ToString(), dtDateTime.ToLongTimeString() };
        }


        /// <summary>
        /// Перводит время из формата UnixTime в обычный формат времение Windows
        /// </summary>
        /// <param name="time">Строка unixtime</param>
        /// <returns></returns>
        public string GetTimeFromUnixTime(string time)
        {
            DateTime fullDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return fullDateTime.AddSeconds(double.Parse(time)).ToLocalTime().ToString();
        }


        /// <summary>
        /// Возвращает список оповещений об ответах других пользователей на записи текущего пользователя.
        /// </summary>
        /// <param name="filters">перечисленные через запятую типы оповещений, которые необходимо получить. В данный момент поддерживаются следующие типы оповещений:
        /// wall — записи на стене пользователя;
        /// mentions — упоминания в записях на стене, в комментариях или в обсуждениях;
        /// comments — комментарии к записям на стене, фотографиям и видеозаписям;
        /// likes — отметки «Мне нравится»;
        /// reposts — скопированные у текущего пользователя записи на стене, фотографии и видеозаписи;
        /// followers — новые подписчики;
        /// friends — принятые заявки в друзья.
        /// Если параметр не задан, то будут получены все возможные типы оповещений.</param>
        /// <param name="count">Указывает, какое максимальное число оповещений следует возвращать, но не более 100. По умолчанию 30.</param>
        /// <param name="start_from"></param>
        /// <param name="start_time"></param>
        /// <param name="end_time"></param>
        /// <returns></returns>
        public IList<NameValueCollection> NotificationsGet(string filters = "", int count = 0, string start_from = "", int start_time = 0, int end_time = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            if (!string.IsNullOrEmpty(filters)) qs["filters"] = filters;
            if (count != 0) qs["count"] = count.ToString();
            if (!string.IsNullOrEmpty(start_from)) qs["start_from"] = start_from.ToString();
            if (count != 0) qs["start_time"] = start_time.ToString();
            if (count != 0) qs["end_time"] = end_time.ToString();

            JObject o = JObject.Parse(this.ExecuteCommand("notifications.get", qs));
            JArray array = (JArray)o["response"]["items"];

            
            IList<NameValueCollection> retFull = new List<NameValueCollection>();
            
            foreach (var row in array)
            {
                NameValueCollection ret = new NameValueCollection();
                //Debug.WriteLine(row["type"] + " - " + GetTimeFromUnixTime(row["date"].ToString()) + " - " + row["feedback"].ToString());

                ret["type"] = row["type"].ToString();
                ret["date"] = GetTimeFromUnixTime(row["date"].ToString());
                ret["feedback"] = row["feedback"].ToString();

                retFull.Add(ret);
            }
            return retFull;
            //return this.ExecuteCommand("notifications.get", qs);
        }

        /// <summary>
        /// Сбрасывает счетчик непросмотренных оповещений об ответах других пользователей на записи текущего пользователя.
        /// </summary>
        /// <returns></returns>
        public string NotificationsMarkAsViewed()
        {
            return this.ExecuteCommand("notifications.markAsViewed", new NameValueCollection());
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="album_id"></param>
        /// <param name="photos_list"></param>
        /// <param name="caption"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public string PhotosSave(int album_id, string photos_list, string caption = "", string latitude = "", string longitude = "", int group_id = 0)
        {
            var uploadServer = PhotosGetUploadServer(album_id);
            //отправка файла на полученый сервер
            var retUploadPhoto = HttpUploadFile(uploadServer.upload_url, photos_list);
            NameValueCollection qs = new NameValueCollection();
            qs["album_id"] = album_id.ToString();
            qs["server"] = retUploadPhoto["server"].ToString();
            qs["photos_list"] = retUploadPhoto["photos_list"].ToString();
            qs["hash"] = retUploadPhoto["hash"].ToString();
            if (!string.IsNullOrEmpty(caption)) qs["caption"] = caption;
            if (!string.IsNullOrEmpty(latitude)) qs["latitude"] = latitude;
            if (!string.IsNullOrEmpty(longitude)) qs["longitude"] = longitude;
            return this.ExecuteCommand("photos.save", qs, "post");
        }


        /// <summary>
        /// Одобряет или создает заявку на добавление в друзья.
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя, которому необходимо отправить заявку, либо заявку от которого необходимо одобрить.</param>
        /// <param name="text">Текст сопроводительного сообщения для заявки на добавление в друзья. Максимальная длина сообщения — 500 символов.</param>
        /// <returns></returns>
        public string FriendsAdd(int user_id, string text = "")
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_id"] = user_id.ToString();
            if (!string.IsNullOrEmpty(text)) qs["text"] = text;
            return this.ExecuteCommand("friends.add", qs, "post");
        }


        /// <summary>
        /// Удаляет пользователя из списка друзей или отклоняет заявку в друзья.
        /// </summary>
        /// <param name="user_id">Идентификатор пользователя, которого необходимо удалить из списка друзей, либо заявку от которого необходимо отклонить. </param>
        /// <returns></returns>
        public string FriendsDel(int user_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["user_id"] = user_id.ToString();
            return this.ExecuteCommand("friends.delete", qs);
        }


        /// <summary>
        /// Возвращает информацию о полученных или отправленных заявках на добавление в друзья для текущего пользователя.
        /// </summary>
        /// <param name="offset">Смещение, необходимое для выборки определенного подмножества заявок на добавление в друзья.</param>
        /// <param name="count">Максимальное количество заявок на добавление в друзья, которые необходимо получить (не более'''1000).</param>
        /// <param name="out_req">0 — возвращать полученные заявки в друзья (по умолчанию), 1 — возвращать отправленные пользователем заявки.</param>
        /// <returns></returns>
        public string FriendsGetRequests(int offset = 0, int count = 100, int out_req = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            if (offset != 0) qs["offset"] = offset.ToString();
            if (count != 100) qs["count"] = count.ToString();
            qs["out_req"] = out_req.ToString();
            return this.ExecuteCommand("friends.getRequests", qs);
        }

        /// <summary>
        /// Возвращает список идентификаторов общих друзей между парой пользователей.
        /// </summary>
        /// <param name="target_uid">Идентификатор пользователя, с которым необходимо искать общих друзей.</param>
        /// <param name="source_uid">Идентификатор пользователя, чьи друзья пересекаются с друзьями пользователя с идентификатором target_uid. Если параметр не задан, то считается, что source_uid равен идентификатору текущего пользователя.</param>
        /// <returns></returns>
        public string FriendsGetMutual(int target_uid, int source_uid = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["target_uid"] = target_uid.ToString();
            if (source_uid != 0) qs["source_uid"] = source_uid.ToString();
            return this.ExecuteCommand("friends.getMutual", qs);
        }

        /// <summary>
        /// Добавляет указанный объект в список Мне нравится текущего пользователя.
        /// </summary>
        /// <param name="type">Тип объекта: post, comment, photo, audio, video, note, photo_comment, video_comment, topic_comment</param>
        /// <param name="owner_id">Идентификатор владельца объекта.</param>
        /// <param name="item_id">Идентификатор объекта.</param>
        /// <returns></returns>
        public string LikesAdd(string type, int owner_id, int item_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["type"] = type;
            qs["owner_id"] = owner_id.ToString();
            qs["item_id"] = item_id.ToString();
            return this.ExecuteCommand("likes.add", qs);
        }


        /// <summary>
        /// Удаляет указанный объект из списка Мне нравится текущего пользователя
        /// </summary>
        /// <param name="type">Тип объекта: post, comment, photo, audio, video, note, photo_comment, video_comment, topic_comment</param>
        /// <param name="owner_id">Идентификатор владельца объекта.</param>
        /// <param name="item_id">Идентификатор объекта.</param>
        /// <returns></returns>
        public string LikesDelete(string type, int owner_id, int item_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["type"] = type;
            qs["owner_id"] = owner_id.ToString();
            qs["item_id"] = item_id.ToString();
            return this.ExecuteCommand("likes.delete", qs);
        }


        /// <summary>
        /// Проверяет, находится ли объект в списке Мне нравится заданного пользователя.
        /// </summary>
        /// <param name="type">Тип объекта: post, comment, photo, audio, video, note, photo_comment, video_comment, topic_comment</param>
        /// <param name="owner_id">Идентификатор владельца объекта.</param>
        /// <param name="item_id">Идентификатор объекта.</param>
        /// <param name="user_id">Идентификатор пользователя, у которого необходимо проверить наличие объекта в списке «Мне нравится». Если параметр не задан, то считается, что он равен идентификатору текущего пользователя.</param>
        /// <returns></returns>
        public string LikesIsLiked(string type, int owner_id, int item_id, int user_id = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["type"] = type;
            qs["owner_id"] = owner_id.ToString();
            qs["item_id"] = item_id.ToString();
            if (user_id != 0) qs["user_id"] = user_id.ToString();
            return this.ExecuteCommand("likes.isLiked", qs);
        }


        /// <summary>
        /// Получает список идентификаторов пользователей, которые добавили заданный объект в свой список Мне нравится.
        /// </summary>
        /// <param name="type">Тип объекта: post, comment, photo, audio, video, note, photo_comment, video_comment, topic_comment</param>
        /// <param name="owner_id">Идентификатор владельца объекта.</param>
        /// <param name="item_id">Идентификатор объекта.</param>
        /// <param name="filter">указывает, следует ли вернуть всех пользователей, добавивших объект в список "Мне нравится" или только тех, которые рассказали о нем друзьям. Параметр может принимать следующие значения: likes, copies.</param>
        /// <param name="offset">Смещение, относительно начала списка, для выборки определенного подмножества. Если параметр не задан, то считается, что он равен 0.</param>
        /// <param name="count">Количество возвращаемых идентификаторов пользователей. Если параметр не задан, то считается, что он равен 100, если не задан параметр friends_only, в противном случае 10. Максимальное значение параметра 1000, если не задан параметр friends_only, в противном случае 100.</param>
        /// <returns></returns>
        public string LikesGetList(string type, int owner_id, int item_id, int extended = 1, string filter = "likes", int offset = 0, int count = 0)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["type"] = type;
            qs["owner_id"] = owner_id.ToString();
            qs["item_id"] = item_id.ToString();
            qs["extended"] = extended.ToString();
            qs["filter"] = filter;
            if (offset != 0) qs["offset"] = offset.ToString();
            if (count != 0) qs["count"] = count.ToString();
            return this.ExecuteCommand("likes.getList", qs);
        }


        /// <summary>
        /// Данный метод позволяет вступить в группу, публичную страницу, а также подтвердить участие во встрече.
        /// </summary>
        /// <param name="group_id">Идентификатор сообщества.</param>
        /// <returns></returns>
        public string GroupsJoin(int group_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["group_id"] = group_id.ToString();
            return this.ExecuteCommand("groups.join", qs);
        }


        /// <summary>
        /// Данный метод позволяет выходить из группы, публичной страницы, или встречи.
        /// </summary>
        /// <param name="group_id">Идентификатор сообщества.</param>
        /// <returns></returns>
        public string GroupsLeave(int group_id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["group_id"] = group_id.ToString();
            return this.ExecuteCommand("groups.leave", qs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public JObject HttpUploadFile(string url, string file, string paramName = "file1", string contentType = "image/jpeg", NameValueCollection nvc = null)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            int ConnectionLimit = Environment.ProcessorCount * 4 + 4;
            wr.ServicePoint.ConnectionLimit = ConnectionLimit;
            //
            //ServicePoint currentServicePoint = wr.ServicePoint;
            //currentServicePoint.ConnectionLimit = 100;
            //

            Stream rs = wr.GetRequestStream();

            if (nvc != null)
            {
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }

            /*
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, @"_square_crop", "1,1,50");
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            */

            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            JObject json = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string s3 = reader2.ReadToEnd();
                //var json = JObject.Parse(HttpUtility.UrlEncode(s3));
                json = JObject.Parse(s3);
                
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally { wresp.Close();  wr = null; }
            return json;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>   
        /// Выполняет запрос к API 
        /// </summary>
        /// <param name="name">Имя api-метода</param>
        /// <param name="qs">Дополнительные параметры</param>
        /// <param name="method">Метод отправки данных (get/post)</param>
        public string ExecuteCommand(string name, NameValueCollection qs, string method = "get")
        {
            string ret = this.ExecuteCommandAsync(name, qs, method);
            if (ret == @"error timeout")
            {
                Thread.Sleep(1000);
                return this.ExecuteCommandAsync(name, qs, method);
            }
            else
            {
                return ret;
            }
            

            /*Command ec = new Command(name, qs);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync(ec);*/

            var cmd = (method == "get") 
                ? string.Format("https://api.vk.com/method/{0}?access_token={1}&v={2}&{3}", name, this.accessToken, versionAPI , string.Join("&", from item in qs.AllKeys select item + "=" + qs[item]))
                : string.Format("https://api.vk.com/method/{0}?access_token={1}&v={2}", name, this.accessToken, versionAPI);

            WebRequest request = WebRequest.Create(cmd);

            ((HttpWebRequest)request).Accept = "application/json";
            ((HttpWebRequest)request).Headers["Accept-Language"] = "en-US";
            ((HttpWebRequest)request).UserAgent = "Spook 0.1-alpha .NET client for VK";
            ((HttpWebRequest)request).KeepAlive = true;
            ((HttpWebRequest)request).Timeout = 25000;
            if (method == "get")
            {
                ((HttpWebRequest)request).ContentType = "application/json";
                ((HttpWebRequest)request).Method = "GET";
            } else {
                ((HttpWebRequest)request).ContentType = "application/x-www-form-urlencoded";
                ((HttpWebRequest)request).Method = "POST";
            }

            if (method != "get")
            {
                var sendString = string.Join("&", from item in qs.AllKeys select item + "=" + (HttpUtility.UrlEncode(qs[item], Encoding.UTF8)));
                byte[] sentData = Encoding.UTF8.GetBytes(sendString);
                request.ContentLength = sentData.Length;
                Stream sendStream = request.GetRequestStream();
                sendStream.Write(sentData, 0, sentData.Length);
                sendStream.Close();
            }

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return HttpUtility.HtmlDecode(responseFromServer);

            //https://msdn.microsoft.com/en-us/library/debx8sh9%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396

            //using (var w = new WebClient())
            /*{
                var json_data = string.Empty;
                try { json_data = w.DownloadString(String.Format("https://api.vk.com/method/{0}?access_token={1}&v=5.30&{2}", name, this.accessToken, String.Join("&", from item in qs.AllKeys select item + "=" + qs[item]))); }
                catch (Exception) { }
                return json_data;
                // if string with JSON data is not empty, deserialize it to class and return its instance 
                //return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }*/
        }



        /// <summary>   
        /// Выполняет запрос к API 
        /// </summary>
        /// <param name="name">Имя api-метода</param>
        /// <param name="qs">Дополнительные параметры</param>
        /// <param name="method">Метод отправки данных (get/post)</param>
        static string ExecuteCommandDelegate(string name, NameValueCollection qs, string accessToken, string method = "get")
        {
            var cmd = (method == "get")
                ? string.Format("https://api.vk.com/method/{0}?access_token={1}&v={2}&{3}", name, accessToken, versionAPI, string.Join("&", from item in qs.AllKeys select item + "=" + qs[item]))
                : string.Format("https://api.vk.com/method/{0}?access_token={1}&v={2}", name, accessToken, versionAPI);

            WebRequest request = WebRequest.Create(cmd);

            ((HttpWebRequest)request).Accept = "application/json";
            ((HttpWebRequest)request).Headers["Accept-Language"] = "en-US";
            ((HttpWebRequest)request).UserAgent = "Spook 0.2-alpha .NET client for VK";
            ((HttpWebRequest)request).KeepAlive = true;
            ((HttpWebRequest)request).Timeout = 25000;
            if (method == "get")
            {
                ((HttpWebRequest)request).ContentType = "application/json";
                ((HttpWebRequest)request).Method = "GET";
            }
            else
            {
                ((HttpWebRequest)request).ContentType = "application/x-www-form-urlencoded";
                ((HttpWebRequest)request).Method = "POST";
            }

            if (method != "get")
            {
                var sendString = string.Join("&", from item in qs.AllKeys select item + "=" + (HttpUtility.UrlEncode(qs[item], Encoding.UTF8)));
                byte[] sentData = Encoding.UTF8.GetBytes(sendString);
                request.ContentLength = sentData.Length;
                Stream sendStream = request.GetRequestStream();
                sendStream.Write(sentData, 0, sentData.Length);
                sendStream.Close();
            }

            try
            {
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                response.Close();
                return HttpUtility.HtmlDecode(responseFromServer);

            }
            catch (Exception ex)
            {
                //return ex.Message;
                return @"error timeout";
            }
        }


        /// <summary>   
        /// Выполняет асинхронный запрос к API 
        /// </summary>
        /// <param name="name">Имя api-метода</param>
        /// <param name="qs">Дополнительные параметры</param>
        /// <param name="method">Метод отправки данных (get/post)</param>
        public string ExecuteCommandAsync(string name, NameValueCollection qs, string method = "get")
        {
            ECDelegate exec = ExecuteCommandDelegate;
            IAsyncResult ar = exec.BeginInvoke(name, qs, this.accessToken, method, null, null);
            while (!ar.IsCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(50);
            }
            string result = exec.EndInvoke(ar);
            return result;
        }


        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            // Получить входные значения
            Command ec = (Command)e.Argument;
            WebRequest request = WebRequest.Create(String.Format("https://api.vk.com/method/{0}?access_token={1}&v=5.30&{2}", ec.name, this.accessToken, String.Join("&", from item in ec.qs.AllKeys select item + "=" + ec.qs[item])));
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            e.Result = HttpUtility.HtmlDecode(responseFromServer);
        }
    }
}
