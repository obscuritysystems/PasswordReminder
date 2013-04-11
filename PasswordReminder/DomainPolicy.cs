using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace PasswordReminder
{

[Flags]
public enum PasswordPolicy
{
  DOMAIN_PASSWORD_COMPLEX=1,
  DOMAIN_PASSWORD_NO_ANON_CHANGE=2, 
  DOMAIN_PASSWORD_NO_CLEAR_CHANGE=4, 
  DOMAIN_LOCKOUT_ADMINS=8,
  DOMAIN_PASSWORD_STORE_CLEARTEXT=16,
  DOMAIN_REFUSE_PASSWORD_CHANGE=32
}

public class DomainPolicy
{
  
	ResultPropertyCollection attribs;

  public DomainPolicy(DirectoryEntry domainRoot)
  {
    string[] policyAttributes = new string[] {
      "maxPwdAge", "minPwdAge", "minPwdLength", 
      "lockoutDuration", "lockOutObservationWindow", 
      "lockoutThreshold", "pwdProperties", 
      "pwdHistoryLength", "objectClass", 
      "distinguishedName"
      };

    //we take advantage of the marshaling with
    //DirectorySearcher for LargeInteger values...
    DirectorySearcher ds = new DirectorySearcher(
      domainRoot,
      "(objectClass=domainDNS)",
      policyAttributes,
      SearchScope.Base
      );

    SearchResult result = ds.FindOne();

    //do some quick validation...							  
    if (result == null)
    {
      throw new ArgumentException(
        "domainRoot is not a domainDNS object."
        );
    }

    this.attribs = result.Properties;
  }

  //for some odd reason, the intervals are all stored
  //as negative numbers. We use this to "invert" them
  private long GetAbsValue(object longInt)
  {
    return Math.Abs((long)longInt);
  }

  public TimeSpan MaxPasswordAge
  {
    get
    {
      string val = "maxPwdAge";
      if (this.attribs.Contains(val))
      {
        long ticks = GetAbsValue(
          this.attribs[val][0]
          );

        if (ticks > 0)
          return TimeSpan.FromTicks(ticks);
      }

      return TimeSpan.MaxValue;
    }
  }

  public PasswordPolicy PasswordProperties
  {
    get
    {
      string val = "pwdProperties";
      //this should fail if not found
      return (PasswordPolicy)this.attribs[val][0];
    }
  }
  
  //truncated for book space
}
}

