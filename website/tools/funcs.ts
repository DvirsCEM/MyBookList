import { send } from "../clientUtilities";

export var getUserSecret = async (): Promise<string | null> => {
  var userSecret = localStorage.getItem("userSecret");
  if (userSecret == null) {
    return null;
  }

  var varified = await send("verifyUser", userSecret);
  if (!varified) {
    localStorage.removeItem("userSecret");
    return null;
  }

  return userSecret;
};
