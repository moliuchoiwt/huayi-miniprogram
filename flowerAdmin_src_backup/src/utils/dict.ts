// ? 系统全局字典

/**
 * @description：用户性别
 */
export const genderType = [
  { label: "男", value: 1 },
  { label: "女", value: 2 }
];

/**
 * @description：用户状态
 */
export const userStatus = [
  { label: "启用", value: 1, tagType: "success" },
  { label: "禁用", value: 0, tagType: "danger" }
];

/**
 * @description：订单状态
 */
export const statusList = [
  { label: "待支付", value: 0, tagType: "info" },
  { label: "待审核", value: 1, tagType: "warning" },
  { label: "已发布", value: 2, tagType: "primary" },
  { label: "进行中", value: 3, tagType: "primary" },
  { label: "待收货", value: 4, tagType: "success" },
  { label: "已完成", value: 5, tagType: "success" },
  { label: "售后中", value: 6, tagType: "danger" },
  { label: "售后完成", value: 7, tagType: "danger" },
  { label: "已取消", value: 8, tagType: "danger" },
  { label: "已驳回", value: 9, tagType: "danger" },
  { label: "已删除", value: 99, tagType: "danger" }
];
