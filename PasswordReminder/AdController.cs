using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Security;
using System.Collections;

namespace PasswordReminder
{
	public class AdController
	{
		
		const int UF_DONT_EXPIRE_PASSWD = 0x10000;
		
		DomainPolicy policy;
		
		DirectoryEntry ADentry = new DirectoryEntry("LDAP://xxxx", "xxx@yyyy.com", "bestpasswordever");				
		
		public AdController ()
		{
			ADentry = new DirectoryEntry("LDAP://xxxx", "xxx@yyyy.com", "bestpasswordever");
			
			setPolicy();
		}
		
		public void setPolicy()
		{
			
    		Domain domain = Domain.GetCurrentDomain();
    		DirectoryEntry root = domain.GetDirectoryEntry();				
			
			policy = new DomainPolicy(root);
			
		}
		
		public void printAllUsers()
		{
			
			ArrayList Users = GetAllADDomainUsers();
			
				foreach(String user in Users)
				{				
					Console.WriteLine(user);
				}
		}
		
		public void printTimeLeftForPassword(String UserName)
		{	
			Console.WriteLine(GetTimeLeft(new DirectoryEntry(findDN(UserName))));			
		}
		
		
		public void printPolicyMaxPasswordAge()
		{
			Console.WriteLine(policy.MaxPasswordAge);	
		}
		
		public void unlockADuserUsingUN(String username)
		{
			unlockADUserUsingDN(findDN(username));
		}
		
		
		public void PrintExpiration(String UserName)
		{			
			Console.WriteLine(GetExpiration(new DirectoryEntry(findDN(UserName))));
		}
		
		public DateTime getExpirationFromUserNameString(String UserName)
		{				
			return GetExpiration(new DirectoryEntry(findDN(UserName)));
		}
		
		public void unlockADUserUsingDN(String userDn)
		{
		    try
		    {
		        DirectoryEntry uEntry = new DirectoryEntry(userDn);
		        uEntry.Properties["LockOutTime"].Value = 0; //unlock account
				
				Console.WriteLine(uEntry.Name);
		        Console.WriteLine(uEntry.Path);
				
				uEntry.CommitChanges(); //may not be needed but adding it anyways		
		        uEntry.Close();
		    }
		    catch (System.DirectoryServices.DirectoryServicesCOMException E)
		    {
		        //DoSomethingWith --> E.Message.ToString();
				Console.WriteLine(E.Message);
		
		    }
		}
	
		public TimeSpan getTimeLeftByUserString(String UserName)
		{
			return GetTimeLeft(new DirectoryEntry(findDN(UserName)));				
		}
		
		public TimeSpan GetTimeLeft(DirectoryEntry user)
	  	{
	    	DateTime willExpire = GetExpiration(user);
		 
		    if (willExpire == DateTime.MaxValue)
		      return TimeSpan.MaxValue;
		 
		    if (willExpire == DateTime.MinValue)
		      return TimeSpan.MinValue;
		 
		    if (willExpire.CompareTo(DateTime.Now) > 0)
		    {
		      //the password has not expired
		      //(pwdLast + MaxPwdAge)- Now = Time Left
		      return willExpire.Subtract(DateTime.Now);
		    }
		 
		    //the password has already expired
		    return TimeSpan.MinValue;
	  	}
  
		private Int64 GetInt64(DirectoryEntry entry, string attr)
		{
			   
			    DirectorySearcher ds = new DirectorySearcher(entry,String.Format("({0}=*)", attr),new string[] { attr },SearchScope.Base);
			      
			    SearchResult sr = ds.FindOne();
			    
			    if (sr != null)
			    {
			      if (sr.Properties.Contains(attr))
			      {
			        return (Int64)sr.Properties[attr][0];
			      }
			    }
			    return -1;
		}
		
		
		public void printPasswordExperation(String UserName) 
		{
			Console.WriteLine(GetExpiration(new DirectoryEntry(findDN(UserName))));				
		}
		
		public DateTime GetExpiration(DirectoryEntry user)
  		{
	    	int flags = (int)user.Properties["userAccountControl"][0];
	 
		    //check to see if password is set to expire
		    if(Convert.ToBoolean(flags & UF_DONT_EXPIRE_PASSWD))
		    {
		      //the user’s password will never expire
		      return DateTime.MaxValue;
		    }
		 
		    long ticks = GetInt64(user, "pwdLastSet");
		 
		    //user must change password at next login
		    if (ticks == 0)
		      return DateTime.MinValue;
		 
		    //password has never been set
		    if (ticks == -1)
		    {
		      throw new InvalidOperationException("User does not have a password");
		    }
	 
	    	//get when the user last set their password;
	    	DateTime pwdLastSet = DateTime.FromFileTime(ticks);
	 
	    	//use our policy class to determine when
	    	//it will expire			
	    	return pwdLastSet.Add(this.policy.MaxPasswordAge);
  		}
		
		public ArrayList GetAllADDomainUsers()
		{
    			ArrayList allUsers = new ArrayList();
			    
    			DirectorySearcher search = new DirectorySearcher();
    			search.Filter = "(&(objectClass=user)(objectCategory=person))";
    			search.PropertiesToLoad.Add("samaccountname");

    			SearchResult result;
    			SearchResultCollection resultCol = search.FindAll();
			
    			if (resultCol != null)
    			{
        			for(int counter=0; counter < resultCol.Count; counter++)
			        {
			            result = resultCol[counter];
			            if (result.Properties.Contains("samaccountname"))
			            {							
			                allUsers.Add((String)result.Properties["samaccountname"][0]);							
			            }
			     	 }
    			}
			
    		return allUsers;
		}
		
		
		public String findEmail(String userName)
		{
				DirectorySearcher search2 = new DirectorySearcher(); 			    
				search2.Filter = String.Format("(SAMAccountName={0})", userName);
			    search2.PropertiesToLoad.Add("mail");
			    
				SearchResult result = search2.FindOne();	
			
			if(result != null)
			{	
				try {
						
				
					return result.Properties["mail"][0].ToString();
				}
				catch(Exception ex)
				{
					Console.WriteLine("Error: finding email address");
					Console.WriteLine(ex.Message);
					return null;	
				}
								
			}else
			{
				return null;	
			}
		}
		
		public String findDN(String userName)
		{
			
			    DirectorySearcher search2 = new DirectorySearcher(); 			    
				search2.Filter = String.Format("(SAMAccountName={0})", userName);
			    search2.PropertiesToLoad.Add("cn");
			    
				SearchResult result = search2.FindOne();	
			
			if(result != null)
			{
				//Console.WriteLine(result.Path);
				return result.Path;			
			}else
			{
				return null;	
			}
			
		}
	}
}

