using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GladcherryShopping.Models;
using GladCherryShopping.Helpers;

namespace DataLayer.Chat
{
    public class ChatHub : Hub
    {
        private string username;
        private ApplicationDbContext db = new ApplicationDbContext();
        private static ConcurrentDictionary<string, string> grants = new ConcurrentDictionary<string, string>(); // <connectionId, userId>
        private static ConcurrentDictionary<string, string> grantsReverse = new ConcurrentDictionary<string, string>(); // <userId, connectionId>
        private static ConcurrentDictionary<string, string> ChatGuests = new ConcurrentDictionary<string, string>(); // <connectionId, guestGeneratedId>
        private static ConcurrentDictionary<string, string> ChatGuestsReverse = new ConcurrentDictionary<string, string>(); // <guestGeneratedId, connectionId>
        private static ConcurrentDictionary<string, string> ChatOperators = new ConcurrentDictionary<string, string>(); // <connectionId, operatorId>
        private static ConcurrentDictionary<string, string> ChatOperatorsReverse = new ConcurrentDictionary<string, string>(); // <operatorId, connectionId>

        public Task JoinRoom(string roomName)
        {
            return Groups.Add(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public override Task OnConnected()
        {
            DoConnect();
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            DoConnect();
            return base.OnReconnected();
        }

        private void DoConnect()
        {
            var type = Context.Request.Headers["Type"];
            if (type == null || type.Length == 0)
            {
                type = Context.QueryString["Type"];
            }
            switch (Convert.ToInt32(type))
            {
                case (int)Roles.ChatRoomOperator:
                    var operatorId = Context.QueryString["UserGeneratedId"];
                    if (operatorId != null)
                    {
                        var currentOperator = db.Users.Where(current => current.Id == operatorId).FirstOrDefault();
                        if (currentOperator != null)
                        {
                            ChatOperators.TryAdd(Context.ConnectionId, operatorId);
                            // for case: disconnected from Client
                            String oldOperatorId;
                            ChatOperatorsReverse.TryRemove(operatorId, out oldOperatorId);
                            ChatOperatorsReverse.TryAdd(operatorId, Context.ConnectionId);
                        }
                    }
                    break;
                case (int)Roles.ChatRoomUser:
                    var value = string.Empty;
                    if (Context.Request.GetHttpContext().Request.Cookies["chruid"] != null)
                    {
                        value = Context.Request.GetHttpContext().Request.Cookies["chruid"].Value;
                    }
                    if (value != string.Empty)
                    {
                        var decryptedUserId = FunctionsHelper.Decrypt(value, "DataLayerco").Split('|')[0];
                        ChatGuests.TryAdd(Context.ConnectionId, decryptedUserId);
                        // for case: disconnected from Client
                        String oldGuestId;
                        ChatGuestsReverse.TryRemove(decryptedUserId, out oldGuestId);
                        ChatGuestsReverse.TryAdd(decryptedUserId, Context.ConnectionId);
                    }
                    else
                    {
                        var userGeneratedId = Context.QueryString["UserGeneratedId"];
                        if (userGeneratedId == null || userGeneratedId == "")
                        {
                            DataLayer.Models.Chat chatRoom = new DataLayer.Models.Chat();
                            chatRoom.OnlineUserId = Guid.NewGuid().ToString();
                            userGeneratedId = chatRoom.OnlineUserId;
                            chatRoom.NewMessagesCount = 0;
                            db.Chats.Add(chatRoom);
                            try
                            {
                                db.SaveChanges();
                                ChatGuests.TryAdd(Context.ConnectionId, userGeneratedId);
                                // for case: disconnected from Client
                                String oldGuestId;
                                ChatGuestsReverse.TryRemove(userGeneratedId, out oldGuestId);
                                ChatGuestsReverse.TryAdd(userGeneratedId, Context.ConnectionId);
                                Random random = new Random();
                                HttpCookie ChatRoomCookie = new HttpCookie("chruid");
                                ChatRoomCookie.Value = FunctionsHelper.Encrypt(chatRoom.OnlineUserId + "|" + chatRoom.Id, "DataLayerco");
                                ChatRoomCookie.Expires = DateTime.Now.AddHours(2);
                                ChatRoomCookie.HttpOnly = false;
                                Context.Request.GetHttpContext().Response.Cookies.Add(ChatRoomCookie);
                            }
                            catch (Exception ex) { }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void sendOperatorChatMessage(string message, string operatorId, string chatId)
        {
            if (message != null && message != "")
            {
                if (operatorId != null && operatorId != "")
                {
                    if (!string.IsNullOrWhiteSpace(chatId))
                    {

                        var chatIdLong = long.Parse(chatId);
                        var chatRoom = db.Chats.Where(current => current.Id == chatIdLong && current.OperatorId == operatorId).FirstOrDefault();
                        if (chatRoom != null)
                        {
                            var targetOperator = db.Users.Where(current => current.Id == operatorId).FirstOrDefault();
                            if (targetOperator != null)
                            {
                                DataLayer.Models.ChatMessage chatMessage = new DataLayer.Models.ChatMessage();
                                chatMessage.Body = message;
                                chatMessage.ChatId = chatRoom.Id;
                                chatMessage.OperatorId = operatorId;
                                try
                                {
                                    db.ChatMessages.Add(chatMessage);
                                    db.SaveChanges();
                                    chatRoom.LastMessage = "اپراتور :" + chatMessage.Body;
                                    chatRoom.NewMessagesCount = 0;
                                    db.Entry(chatRoom).State = EntityState.Modified;
                                    db.SaveChanges();
                                    string userConnectionId;
                                    if (ChatGuestsReverse.ContainsKey(chatRoom.OnlineUserId))
                                    {
                                        ChatGuestsReverse.TryGetValue(chatRoom.OnlineUserId, out userConnectionId);
                                        if (userConnectionId != null && userConnectionId != "")
                                        {
                                            Clients.Client(userConnectionId).attachOperatorMessage(targetOperator.ProfileImage ?? "/images/chat_operator.png", targetOperator.FirstName + " " + targetOperator.LastName, message, DateTime.Now.Hour + ":" + DateTime.Now.Minute);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        var tchatroom = db.Chats.Where(current => current.Id == chatIdLong).FirstOrDefault();
                        if (tchatroom != null && tchatroom.OperatorId == null)
                        {
                            tchatroom.NewMessagesCount = 0;
                            tchatroom.OperatorId = operatorId;
                            db.Entry(tchatroom).State = EntityState.Modified;
                            db.SaveChanges();
                            var targetOperator = db.Users.Where(current => current.Id == operatorId).FirstOrDefault();
                            if (targetOperator != null)
                            {
                                DataLayer.Models.ChatMessage chatMessage = new DataLayer.Models.ChatMessage();
                                chatMessage.Body = message;
                                chatMessage.ChatId = tchatroom.Id;
                                chatMessage.OperatorId = operatorId;
                                try
                                {
                                    db.ChatMessages.Add(chatMessage);
                                    db.SaveChanges();
                                    tchatroom.LastMessage = "اپراتور : " + chatMessage.Body;
                                    db.Entry(tchatroom).State = EntityState.Modified;
                                    db.SaveChanges();
                                    string userConnectionId;
                                    if (ChatGuestsReverse.ContainsKey(tchatroom.OnlineUserId))
                                    {
                                        ChatGuestsReverse.TryGetValue(tchatroom.OnlineUserId, out userConnectionId);
                                        if (userConnectionId != null && userConnectionId != "")
                                        {
                                            Clients.Client(userConnectionId).attachOperatorMessage(targetOperator.ProfileImage, targetOperator.FirstName + " " + targetOperator.LastName, message, DateTime.Now.Hour + ":" + DateTime.Now.Minute);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        public void sendGuestChatMessage(string message, string guestGeneratedId)
        {
            if (guestGeneratedId != null && guestGeneratedId != "")
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    String DecryptedValue = FunctionsHelper.Decrypt(guestGeneratedId, "DataLayerco");
                    string[] values = DecryptedValue.Split('|');
                    String OnlineUserId = values[0].ToString();
                    long ChatId = long.Parse(values[1]);
                    var targetChatRoom = db.Chats.Where(current => current.OnlineUserId == OnlineUserId && current.Id == ChatId).FirstOrDefault();
                    if (targetChatRoom != null)
                    {
                        // save chat message in the database
                        DataLayer.Models.ChatMessage chatMessage = new DataLayer.Models.ChatMessage();
                        chatMessage.Body = message;
                        chatMessage.ChatId = targetChatRoom.Id;
                        chatMessage.OnlineUserId = guestGeneratedId;
                        try
                        {
                            db.ChatMessages.Add(chatMessage);
                            db.SaveChanges();
                            targetChatRoom.LastMessage = chatMessage.Body;
                            targetChatRoom.DateTimeLastModified = DateTime.Now;
                            targetChatRoom.NewMessagesCount++;
                            db.Entry(targetChatRoom).State = EntityState.Modified;
                            db.SaveChanges();
                            if (targetChatRoom.OperatorId != null)
                            {
                                string operatorConnectionId;
                                if (ChatOperatorsReverse.ContainsKey(targetChatRoom.OperatorId))
                                {
                                    ChatOperatorsReverse.TryGetValue(targetChatRoom.OperatorId, out operatorConnectionId);
                                    Clients.Client(operatorConnectionId).attachGuestMessage(message, targetChatRoom.Id.ToString(), targetChatRoom.NewMessagesCount.ToString());
                                }
                                // send signal to specific operator
                            }
                            else
                            {
                                // send signal to all operators
                                Clients.All.attachGuestMessage(message, targetChatRoom.Id.ToString(), targetChatRoom.NewMessagesCount.ToString());
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        public static void PushNotificaions(string message)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.broadcastMessage(new ChatMessage() { UserName = string.Empty, Message = message, Type = 1 });
        }
        public static ConcurrentDictionary<String, String> getGrants()
        {
            return grantsReverse;
        }
        public static String getConnectionId(String UserId)
        {
            String ConnectionId;
            grantsReverse.TryGetValue(UserId, out ConnectionId);
            return ConnectionId;
        }
        public void Send(string message)
        {
            // Call the broadcastMessage method to update clients.            
            //string fromUser;
            //FromUsers.TryGetValue(Context.ConnectionId, out fromUser);
            //Clients.AllExcept(Context.ConnectionId).broadcastMessage(new ChatMessage() { UserName = fromUser, Message = message });
        }

        public void SendChatMessage(string to, string message)
        {
            //FromUsers.TryGetValue(Context.ConnectionId, out userName);
            //string receiver_ConnectionId;
            //ToUsers.TryGetValue(to, out receiver_ConnectionId);

            //if (receiver_ConnectionId != null && receiver_ConnectionId.Length > 0)
            //{
            //    Clients.Client(receiver_ConnectionId).broadcastMessage(new ChatMessage() { UserName = userName, Message = message });
            //}
        }
    }

    public enum Roles
    {
        Customer = 1,
        HomeKar = 2,
        ChatRoomUser = 3,
        ChatRoomOperator = 4
    }

    public class ChatMessage
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
    }
}