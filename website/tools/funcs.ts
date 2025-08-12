import { send } from "../clientUtilities";

export var getUserId = async (): Promise<string | null> => {
  var userId = localStorage.getItem("userId");
  if (userId == null) {
    return null;
  }

  var varified = await send("verifyUser", userId);
  if (!varified) {
    localStorage.removeItem("userId");
    return null;
  }

  return userId;
};
