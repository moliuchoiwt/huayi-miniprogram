<template>
  <div class="message">
    <el-popover placement="bottom" :width="310" trigger="click" @show="showPopover" @hide="showHide">
      <template #reference>
        <el-badge :value="count" class="item">
          <i :class="'iconfont icon-xiaoxi'" class="toolBar-icon"></i>
        </el-badge>
      </template>
      <el-tabs v-model="activeName">
        <!-- <el-tab-pane label="通知(0)" name="first">
          <div class="message-list">
            <div class="message-item">
              <img src="@/assets/images/msg01.png" alt="" class="message-icon" />
              <div class="message-content">
                <span class="message-title">一键三连🧡</span>
                <span class="message-date">一分钟前</span>
              </div>
            </div>
          </div>
        </el-tab-pane>
        <el-tab-pane label="消息(0)" name="second">
          <div class="message-empty">
            <img src="@/assets/images/notData.png" alt="notData" />
            <div>暂无消息</div>
          </div>
        </el-tab-pane> -->
        <el-tab-pane :label="`待办(${thirdCount})`" name="third">
          <div class="block">
            <div class="message-list" v-if="thirdCount > 0">
              <div
                class="message-item"
                v-for="item in messageList.third"
                :key="item.title"
                v-show="item.total > 0"
                @click="toPush(item)"
              >
                <img :src="item.icon" alt="" class="message-icon" />
                <div class="message-content">
                  <span class="message-title"> {{ item.title }} </span>
                  <span class="message-date"> {{ item.date }} </span>
                </div>
              </div>
            </div>
            <div class="message-empty" v-else>
              <img src="@/assets/images/notData.png" alt="notData" />
              <div>暂无待办</div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-popover>
  </div>
</template>

<script setup lang="ts">
import { sysGoodsOrderApi, sysShopApi } from "@/api/api";
import router from "@/routers";
import { useUserStore } from "@/stores/modules/user";
import { useAuthStore } from "@/stores/modules/auth";
import { computed, ref } from "vue";
import msg01 from "@/assets/images/msg01.png";
import msg02 from "@/assets/images/msg02.png";
import msg03 from "@/assets/images/msg03.png";
import msg04 from "@/assets/images/msg04.png";
import msg05 from "@/assets/images/msg05.png";
const msgList = [msg01, msg02, msg03, msg04, msg05];
const userStore = useUserStore();
const authStore = useAuthStore();
const activeName = ref("third");

// 初始化消息列表
const initMessageList = () => {
  messageList.value = {
    third: [] // 待办消息列表
  };
};
// 消息列表
const messageList = ref<Record<string, { total: number; title: string; date: string; link: string; icon: string }[]>>({});
// 待办总数
const thirdCount = computed(() => messageList.value.third.reduce((pre, cur) => pre + cur.total, 0) || 0);
// 消息总数
const count = computed(() => thirdCount.value || 0);
// 获取消息需要的权限keys
const getKeys: any = [
  [
    "shopCheck",
    () => {
      // 店铺
      sysShopApi.list({ auditState: 0, pageNum: 1, pageSize: 1000 }).then(res => {
        messageList.value.third.push({
          total: res.data.total,
          title: "待办【店铺】",
          date: `待审核店铺共${res.data.total}条`,
          link: "/user/shopCheck/list",
          icon: msgList[3]
        });
      });
    }
  ],
  [
    "orderList",
    () => {
      // 订单
      sysGoodsOrderApi.list({ status: 1, pageNum: 1, pageSize: 1000 }).then(res => {
        messageList.value.third.push({
          total: res.data.total,
          title: "待办【订单】",
          date: `待审核订单共${res.data.total}条`,
          link: "/order/list",
          icon: msgList[2]
        });
      });
    }
  ]
];
// 当前账号权限keys
const authKeys = Object.keys(authStore.authButtonList);
// 过滤需要的权限keys
const filterKeys = getKeys.filter(item => authKeys.includes(item[0]));
// 获取信息列表
const getList = () => {
  if (userStore.token) {
    initMessageList();
    // 遍历获取消息
    filterKeys.forEach(item => item[1]());
  }
};
initMessageList();
getList();
let interval: any = setInterval(() => {
  getList();
}, 10000);
// 弹框显示
const showPopover = () => {
  clearInterval(interval);
  getList();
  return;
};
// 弹框隐藏
const showHide = () => {
  getList();
  interval = setInterval(() => {
    getList();
  }, 10000);
};

const toPush = (item: { link: string }) => {
  router.push(item.link);
};
</script>

<style scoped lang="scss">
.block {
  width: 100%;
  max-height: 260px;
  overflow: auto;
}
.message-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 260px;
  line-height: 45px;
}
.message-list {
  display: flex;
  flex-direction: column;
  .message-item {
    display: flex;
    align-items: center;
    padding: 10px 0;
    cursor: pointer;
    border-bottom: 1px solid var(--el-border-color-light);
    &:last-child {
      padding-bottom: 0;
      border: none;
    }
    .message-icon {
      width: 40px;
      height: 40px;
      margin: 0 10px 0 5px;
    }
    .message-content {
      display: flex;
      flex-direction: column;
      .message-title {
        margin-bottom: 5px;
      }
      .message-date {
        font-size: 12px;
        color: var(--el-text-color-secondary);
      }
    }
  }
}
</style>
