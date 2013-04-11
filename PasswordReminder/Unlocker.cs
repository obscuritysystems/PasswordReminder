using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;


namespace PasswordReminder
{
	public class Unlocker
	{
		Boolean Debug;
		List<String> users;  
		AdController ADcon;
		String strProvider = "";
		System.Diagnostics.EventLog Event;
		
		
		ConfigFileReader configfile;
		
		
		public Unlocker ()
		{
			ADcon = new AdController();
			loadConfig();
		}
		
				
		
		public Unlocker (System.Diagnostics.EventLog E)
		{
			Event = E;
			ADcon = new AdController();
			loadConfig();
		}
		
		
		public void loadConfig()			
		{
			String configFilePath = "config.ini";
			try
			{
			configfile = new ConfigFileReader("config.ini");
			
			if ( configfile.getConfigOption("Debug").ToUpper().Trim() == "TRUE")

		            { Debug = true;  }

        		    else

		            { Debug = false; }
			
			
			strProvider = "Data Source="+configfile.getConfigOption("MYSQLDB_server") +
				";Database="+configfile.getConfigOption("MYSQLDB_database")+
				";User ID="+configfile.getConfigOption("MYSQLDB_uid")+
				";Password="+configfile.getConfigOption("MYSQLDB_password")+"";
			Console.WriteLine(strProvider);
				
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				
				if(Event != null)
				{
					Event.WriteEntry("Cannot Load Config File:" + ex.Message,System.Diagnostics.EventLogEntryType.Error);	
				}
			}
			
		}
		
		public void printUsers()
		{
			foreach(String s in users)
			{
				Console.WriteLine(s);	
			}
		}
		
		public void unlockUserByUName(String userName)
		{
			ADcon.unlockADuserUsingUN(userName);							
		}
		
		public void unlockUsers()
		{			
			foreach(String s in users)
			{
				ADcon.unlockADuserUsingUN(s);	
				if(Event != null)
				{
					Event.WriteEntry("Unlocking user: "+s,System.Diagnostics.EventLogEntryType.Information);	
				}
			}
		}
		
		
		public void deleteFromDB()
		{
			foreach(String s in users)
			{
				try{
		
					MySqlConnection myConnection = new MySqlConnection(strProvider);
					String sql = "DELETE FROM unlockuser where username=?uname";
						
				 	myConnection.Open();
					MySqlCommand m = new MySqlCommand(sql,myConnection);										    	
					m.Parameters.Add(new MySqlParameter("?uname",s));									
					m.Prepare();				
					m.ExecuteNonQuery();
			  		m.Connection.Close();
					
				}catch(Exception ex){				
					Console.WriteLine(ex.Message);			
					Console.WriteLine(ex.HelpLink);
				}
			}
			
		}
		
		public void checkForAccounts()
		{
			users = new List<String>();			
			try{
				MySqlConnection myConnection = new MySqlConnection(strProvider);	
				String sql = "SELECT * FROM unlockuser";
						
				myConnection.Open();
				MySqlCommand m = new MySqlCommand(sql,myConnection);
				MySqlDataReader myReader = m.ExecuteReader(); 
		
				  try { 
				    
					while (myReader.Read()) { 
				      users.Add(myReader.GetString("username")); 						
				    } 
				  } 
				catch(Exception ex){
						Console.WriteLine(ex.Message);				
				}
				finally{ 
					  // always call Close when done reading. 
					  myReader.Close(); 
					  // always call Close when done reading. 
					  myConnection.Close(); 
				} 		
			}
			catch(Exception ex){
						Console.WriteLine(ex.Message);				
			}				
		}
				
		
	}
}

