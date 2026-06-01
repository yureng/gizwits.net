
namespace Gizwits
{
    public class MessageCode
    {
        private MessageCode() { }

        public static string Translate(string m, string c = null)
        {
            if (String.IsNullOrEmpty(m)) return m;

            if (c == null || !c.StartsWith("zh"))
                return TrimFormat(m);

            switch (m)
            {
                case "GIZ_SDK_SUCCESS": //return "执行成功";
                    return null;

                case "GIZ_SDK_NEED_UPDATE_TO_LATEST":
                    return "SDK需要升级到最新版本";
                case "GIZ_SDK_CLIENT_NOT_AUTHEN":
                    return "客户端未认证";
                case "GIZ_SDK_UDP_PORT_BIND_FAILED":
                    return "UDP 端口绑定失败";
                case "GIZ_SDK_PARAM_INVALID":
                    return "APP 传入参数无效";
                case "GIZ_SDK_LOG_PATH_INVALID":
                    return "日志路径无效";

                case "GIZ_SDK_DEVICE_CONFIG_SEND_FAILED":
                    return "设备配置信息发送失败";
                case "GIZ_SDK_DEVICE_CONFIG_IS_RUNNING":
                    return "设备正在配置";
                case "GIZ_SDK_DEVICE_CONFIG_TIMEOUT":
                    return "设备配置超时";
                case "GIZ_SDK_DEVICE_NOT_SUBSCRIBED":
                    return "设备未订阅";
                case "GIZ_SDK_DEVICE_NO_RESPONSE":
                    return "设备未响应";
                case "GIZ_SDK_DEVICE_NOT_READY":
                    return "设备未就绪";
                case "GIZ_SDK_DEVICE_NOT_BINDED":
                    return "设备未绑定";
                case "GIZ_SDK_DEVICE_CONTROL_WITH_INVALID_COMMAND":
                    return "设备控制指令中包含无效指令";
                case "GIZ_SDK_DEVICE_CONTROL_FAILED":
                    return "设备控制指令执行失败";
                case "GIZ_SDK_DEVICE_GET_STATUS_FAILED":
                    return "设备状态查询失败";
                case "GIZ_SDK_DEVICE_CONTROL_VALUE_TYPE_ERROR":
                    return "设备控制指令参数类型错误";
                case "GIZ_SDK_DEVICE_CONTROL_VALUE_OUT_OF_RANGE":
                    return "设备控制指令参数值不在有效范围内";
                case "GIZ_SDK_DEVICE_CONTROL_NOT_WRITABLE_COMMAND":
                    return "设备控制指令中包含不可写指令";
                case "GIZ_SDK_BIND_DEVICE_FAILED":
                    return "设备绑定失败";
                case "GIZ_SDK_UNBIND_DEVICE_FAILED":
                    return "设备解绑失败";

                case "GIZ_SDK_BLE_DEVICE_CONNECT_FAILED":
                    return "蓝牙设备连接失败";
                case "GIZ_SDK_BLE_BLUETOOTH_FUNCTION_NOT_TURNED_ON":
                    return "蓝牙功能没打开";
                case "GIZ_SDK_BLE_PARAM_UUID_INFO_REQUIRED":
                    return "服务角色特征值不能为空";
                case "GIZ_SDK_BLE_PARAM_LTK_REQUIRED":
                    return "通信密钥LTK不能为空";
                case "GIZ_SDK_BLE_UNFIND_DEVICE_PERIPHERAL":
                    return "没有找到蓝牙设备对应的外设";
                case "GIZ_SDK_BLE_LOGIN_FAILED":
                    return "登录蓝牙设备失败";
                case "GIZ_SDK_BLE_SEARCH_DEVICE_STOPPED":
                    return "搜索蓝牙设备操作已经停止";
                case "GIZ_SDK_BLE_CANNOT_FIND_DEVICE_SERVER_OR_CHARACT ERISTICS":
                    return "查找不到设备的服务和角色特征值";
                case "GIZ_SDK_BLE_DEVICE_IS_DISCONNECTED":
                    return "设备处于断开连接状态";

                case "GIZ_SDK_ONBOARDING_STOPPED":
                    return "设备配网中断";
                case "GIZ_SDK_ONBOARDING_WIFI_IS_5G":
                    return "当前配网路由是5G";

                case "GIZ_SDK_CONNECTION_TIMEOUT":
                    return "连接超时";
                case "GIZ_SDK_CONNECTION_REFUSED":
                    return "连接被拒绝";
                case "GIZ_SDK_CONNECTION_ERROR":
                    return "连接错误";
                case "GIZ_SDK_CONNECTION_CLOSED":
                    return "连接被关闭";
                case "GIZ_SDK_SSL_HANDSHAKE_FAILED":
                    return "ssl 握手失败";
                case "GIZ_SDK_DEVICE_LOGIN_VERIFY_FAILED":
                    return "设备登录验证失败";
                case "GIZ_SDK_INTERNET_NOT_REACHABLE":
                    return "当前外网不可达";
                case "GIZ_SDK_HTTP_SERVER_NO_ANSWER":
                    return "http 服务无响应";
                case "GIZ_SDK_HTTP_REQUEST_FAILED":
                    return "http 请求失败";

                case "GIZ_SDK_USER_ID_INVALID":
                    return "用户 ID 无效";
                case "GIZ_SDK_TOKEN_INVALID":
                    return "用户 token 无效";

                case "GIZ_SDK_GROUP_FAILED_DELETE_DEVICE":
                    return "组设备删除失败";
                case "GIZ_SDK_GROUP_FAILED_ADD_DEVICE":
                    return "组设备添加失败";
                case "GIZ_SDK_GROUP_GET_DEVICE_FAILED":
                    return "组设备获取失败";

                case "GIZ_SDK_APK_PERMISSION_NOT_SET":
                    return "app 权限不足";

                case "GIZ_SDK_REQUEST_TIMEOUT":
                    return "操作超时";

                case "GIZ_SDK_PHONE_NOT_CONNECT_TO_SOFTAP_SSID":
                    return "手机没有连接软 AP 热点";
                case "GIZ_SDK_DEVICE_CONFIG_SSID_NOT_MATCHED":
                    return "手机热点和要配置的路由 ssid 不匹配";
                case "GIZ_SDK_NOT_IN_SOFTAPMODE":
                    return "设备不在 softap 模式";
                case "GIZ_SDK_CONFIG_NO_AVAILABLE_WIFI":
                    return "设备配置时无可用 wifi";

                case "GIZ_SDK_START_SUCCESS":
                    return "SDK 启动成功";

                case "GIZ_OPENAPI_MAC_ALREADY_REGISTERED":
                    return "此机器地址（MAC）已经被绑定";
                case "GIZ_OPENAPI_TOKEN_INVALID":
                    return "令牌失效";
                case "GIZ_OPENAPI_USER_NOT_EXIST":
                    return "用户不存在";
                case "GIZ_OPENAPI_SERVER_ERROR":
                    return "服务器出错";
                case "GIZ_OPENAPI_CODE_EXPIRED":
                    return "验证码过期";
                case "GIZ_OPENAPI_CODE_INVALID":
                    return "验证码无效";
                case "GIZ_OPENAPI_DEVICE_NOT_FOUND":
                    return "设备未找到";
                case "GIZ_OPENAPI_FORM_INVALID":
                    return "提交的数据表单报文无效";

                case "GIZ_OPENAPI_DEVICE_NOT_BOUND":
                    return "设备未绑定";
                case "GIZ_OPENAPI_PHONE_UNAVALIABLE":
                    return "电话号码不可用（已被注册）";
                case "GIZ_OPENAPI_USERNAME_UNAVALIABLE":
                    return "用户名不可用（已存在此用户）";
                case "GIZ_OPENAPI_USERNAME_PASSWORD_ERROR":
                    return "用户名或密码错误";
                case "GIZ_OPENAPI_SEND_COMMAND_FAILED":
                    return "命令发送失败";
                case "GIZ_OPENAPI_EMAIL_UNAVALIABLE":
                    return "电子邮件地址不可用（已被注册）";
                case "GIZ_OPENAPI_DEVICE_DISABLED":
                    return "设备已禁用";
                case "GIZ_OPENAPI_USER_INVALID":
                    return "用户无效";
                case "GIZ_OPENAPI_SEND_SMS_FAILED":
                    return "短信发送失败";
                case "GIZ_OPENAPI_DEVICE_OFFLINE":
                    return "设备已离线";
                case "GIZ_OPENAPI_CANNOT_SHARE_TO_SELF":
                    return "不能共享设备给自己";
                case "GIZ_OPENAPI_ONLY_OWNER_CAN_SHARE":
                    return "只有设备所有者能共享设备";
                case "GIZ_OPENAPI_ONLY_SELF_CAN_MODIFY_ALIAS":
                    return "只有设备所有者能更改设备";

                default:
                    return TrimFormat(m);
            }
        }

        public static string Translate(int code)
        {
            switch(code)
            {
                case 9001:
                    return Translate("GIZ_OPENAPI_MAC_ALREADY_REGISTERED");
                case 9004:
                    return Translate("GIZ_OPENAPI_TOKEN_INVALID");
                case 9008:
                    return Translate("GIZ_OPENAPI_SERVER_ERROR");
                case 9009:
                    return Translate("GIZ_OPENAPI_CODE_EXPIRED");
                case 9010:
                    return Translate("GIZ_OPENAPI_CODE_INVALID");
                case 9015:
                    return Translate("GIZ_OPENAPI_FORM_INVALID");

                case 9018:
                    return Translate("GIZ_OPENAPI_PHONE_UNAVALIABLE");
                case 9019:
                    return Translate("GIZ_OPENAPI_USERNAME_UNAVALIABLE");
                case 9020:
                    return Translate("GIZ_OPENAPI_USERNAME_PASSWORD_ERROR");
                case 9021:
                    return Translate("GIZ_OPENAPI_SEND_COMMAND_FAILED");
                case 9022:
                    return Translate("GIZ_OPENAPI_EMAIL_UNAVALIABLE");
                case 9037:
                    return Translate("GIZ_OPENAPI_SEND_SMS_FAILED");
                default:
                    return null;
            }
        }

        static string TrimFormat(string m)
        {
            if (m.StartsWith("GIZ_SDK_"))
                m = m.Substring(8);
            else if (m.StartsWith("GIZ_OPENAPI_"))
                m = m.Substring(12);

            return m.Replace('_', ' ');
        }
    }
}
