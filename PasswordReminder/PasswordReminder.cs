using System;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Net.Mail;

namespace PasswordReminder
{
	public class PasswordReminder
	{
		
		Boolean Debug;
		
		ArrayList Users;  
		AdController ADcon;
		System.Diagnostics.EventLog Event;
		ConfigFileReader configFile;
				
		String strProvider = "";
		String debugEmail = "";
		String adminEmail = "";
		String systemEmailAddress = "";
		String SmtpClientIp = "";
		
		TimeSpan reminderTimeInDays;
		
		int expireInDays = 4; 
			
		public PasswordReminder()
		{
			ADcon = new AdController();					
			Users = ADcon.GetAllADDomainUsers();
			loadConfig();						
		}
		
		public void checkTime()
		{
						
			int expireCount = 0;
			int aboutToExpire = 0;
			
			String adminEmailBody = "";			
			String PastExpire = "Users whose password has expired" + "<br/><br/>";
			
			reminderTimeInDays = TimeSpan.FromDays(expireInDays);	
			
			adminEmailBody += "Remidner time is set to: " + reminderTimeInDays + "<br/><br/>";
			
			adminEmailBody += "Users whose password is about to expire. <br/><br/>";
			
			foreach(String user in Users)
			{
				
				TimeSpan timeleft = ADcon.getTimeLeftByUserString(user);
				DateTime experationDate = ADcon.getExpirationFromUserNameString(user);			
					
				
				String emailAddress = ADcon.findEmail(user);	
				
				if(timeleft <= reminderTimeInDays && timeleft >= TimeSpan.Zero)
				{
					aboutToExpire++;
					
																		
					
					String message = "Your password will expire on "+experationDate+" please go and change your password <br> Thank you!";
					
					adminEmailBody += "Will expire soon.<br/><br/>";					
					adminEmailBody += user + "<br> Time Left: " + timeleft + "<br> Date: "+ experationDate+"<br/>";
					adminEmailBody += "Email: "+emailAddress + "<br/><br/>";
															
					DebugConsole("remidner time in days: " + reminderTimeInDays);	
					DebugConsole(user + ": Time Left: " + timeleft + " Date: "+ experationDate );
					DebugConsole("reset password: " + user);						
					DebugConsole("Email: "+emailAddress);
					DebugConsole("");										
					
					if(Debug)
					{
						sendEmailReminder(debugEmail,systemEmailAddress,"  password is about to expire",message);
					}
					else
					{
						if(emailAddress != "")
						{
							sendEmailReminder(emailAddress,systemEmailAddress," password is about to expire",message);
						}
					}
				}								
				
				if(timeleft < TimeSpan.Zero)
				{
					PastExpire += " Expired.<br/></br>";					
					PastExpire += user + " <br> Expired for: " + timeleft + " <br> Date:"+ experationDate+"<br/>";
					PastExpire += "Email: "+emailAddress + "<br/><br/>";
					expireCount++;
				}
			}
			
			sendEmailReminder(adminEmail,systemEmailAddress,"Password Expire Report - About to Expire: " +aboutToExpire + " Already Expired: " +expireCount+ " ",adminEmailBody += PastExpire);
			
		}
		
		public void DebugConsole(String info)
		{
			if(Debug)
			{
					Console.WriteLine(info);
			}
			
		}
		
		public void loadConfig()			
		{
			String configFilePath = @"C:\batches\passwordreminder\config.ini";
			
			try
			{
			
			configFile = new ConfigFileReader(configFilePath);
			
			if ( configFile.getConfigOption("Debug").ToUpper().Trim() == "TRUE")

		            { Debug = true;  }

        		    else

		            { Debug = false; }
			
			
				/*strProvider = "Data Source="+configFile.getConfigOption("MYSQLDB_server") +
				";Database="+configFile.getConfigOption("MYSQLDB_database")+
				";User ID="+configFile.getConfigOption("MYSQLDB_uid")+
				";Password="+configFile.getConfigOption("MYSQLDB_password")+"";*/
								
				DebugConsole(strProvider);
				
				SmtpClientIp = configFile.getConfigOption("SMTP_server");
				expireInDays = Convert.ToInt32(configFile.getConfigOption("TIME_expireInDays"));
				debugEmail = configFile.getConfigOption("DEBUG_email");
				adminEmail = configFile.getConfigOption("ADMINISTRATION_email");
				systemEmailAddress =  configFile.getConfigOption("SMTP_systemEmailAddress");
				
				
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Environment.Exit(0);
				
				if(Event != null)
				{
					Event.WriteEntry("Cannot Load Config File:" + ex.Message,System.Diagnostics.EventLogEntryType.Error);
					Environment.Exit(0);
				}
			}
			
		}
		
		public void sendEmailReminder(String toEmailAddress,String fromEmailAddress,String Subject, String body)
		{
			try
            {
                SmtpClient SmtpServer = new SmtpClient(SmtpClientIp);
				
				MailMessage mail = new MailMessage();                
                mail.From = new MailAddress(fromEmailAddress);
                mail.To.Add(toEmailAddress);
                mail.Subject = Subject;
                mail.IsBodyHtml = true;                
                mail.Body = body;
				
                SmtpServer.Port = 25;                
                SmtpServer.Send(mail);                
            }
            catch (Exception ex)
            {
                DebugConsole(ex.ToString());
            }
			
		}
			
		
		public String getEmailAddressFromMysqlDB(String user)
		{
			String emailAddress = "";
			
			try{		
					MySqlConnection myConnection = new MySqlConnection(strProvider);
					String sql = "select EmailAddress from user where Username=?uname";
						
				 	myConnection.Open();
					MySqlCommand m = new MySqlCommand(sql,myConnection);										    	
					m.Parameters.Add(new MySqlParameter("?uname",user));									
					m.Prepare();												  						
					MySqlDataReader myReader = m.ExecuteReader(); 
		
				  try { 
				    
					while (myReader.Read()) { 				      
						emailAddress = myReader.GetString("EmailAddress"); 
				    } 
				  } 
				catch(Exception ex){
						DebugConsole(ex.Message);				
				}
				finally{ 
					
					  myReader.Close(); 					  
					  myConnection.Close(); 
					  m.Connection.Close();	
				} 		
			}
			catch(Exception ex){				
			
					DebugConsole(ex.Message);			
					DebugConsole(ex.HelpLink);
			}	
			finally
			{
				
			}
			
			return emailAddress;
		}
		
	}
}

