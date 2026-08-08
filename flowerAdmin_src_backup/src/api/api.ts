import http from "@/api";
let sysapi = "/sysapi";
// 接口代码提示
type apiStr =
  | "list"
  | "operation"
  | "delete"
  | "Login"
  | "UserMenuList"
  | "AuthButtons"
  | "LoginOut"
  | "WebUploadFile"
  | "TreeList"
  | "GetSysConfig"
  | "OperationSysConfig"
  | "GiveCouponToUser"
  | "AuditWithdrawal"
  | "OfflinePayment"
  | "ConfirmRefund"
  | "ConfirmDeliver"
  | "AdminInfo";
// 函数封装 - 自动创建后缀
const apiFun = (url: string) => {
  let objTemp: Record<apiStr, (params?: any) => Promise<any>> = {} as any;
  for (const key of [
    "list",
    "operation",
    "delete",
    "Login",
    "UserMenuList",
    "AuthButtons",
    "LoginOut",
    "WebUploadFile",
    "TreeList",
    "GetSysConfig",
    "OperationSysConfig",
    "GiveCouponToUser",
    "AuditWithdrawal",
    "OfflinePayment",
    "ConfirmRefund",
    "ConfirmDeliver",
    "AdminInfo"
  ]) {
    objTemp[key] = (data: any = {}, loading = false) =>
      http.post(`${sysapi}/${url}/${key}`, data, { loading, cancel: !["WebUploadFile"].includes(key) });
  }
  return objTemp;
};

// 文件上传
export const uploadApi = apiFun("Upload");
// 登录
export const sysLoginApi = apiFun("SysLogin");
// 菜单
export const sysMenuApi = apiFun("SysMenu");
// 账号
export const sysUserApi = apiFun("SysUser");
// 角色
export const sysRoleApi = apiFun("SysRole");
// 用户
export const sysUserInfoApi = apiFun("SysUserInfo");
// 商家
export const sysShopApi = apiFun("SysShop");
// 轮播图
export const sysBannerApi = apiFun("SysBanner");
// 商品
export const sysGoodsApi = apiFun("SysGoods");
// 商品分类
export const sysClassApi = apiFun("sysClass");
// 会员
export const sysUserGradeApi = apiFun("SysUserGrade");
// 文章
export const sysArticleApi = apiFun("SysArticle");
// 物流
export const sysExpressApi = apiFun("SysExpress");
// 意见反馈
export const sysFeedbackApi = apiFun("SysFeedback");
// 提现 --AuditWithdrawal 订单退款回复 (auditIntro:审核信息 Id status  1.通过 2.驳回)
export const sysWithdrawalApi = apiFun("SysWithdrawal");
// 订单 --ConfirmRefund 退款回复 (auditIntro:审核信息 Id status  1.通过 2.驳回)
export const sysGoodsOrderApi = apiFun("SysGoodsOrder");
// 任务订单
export const sysTaskOrderApi = apiFun("SysTaskOrder");
// 钱包日志
export const sysWalletLogApi = apiFun("sysWalletLog");
