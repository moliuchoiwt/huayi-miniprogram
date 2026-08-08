<template>
	<view>
		<view class="header">
			<view class="name">总账户</view>
			<view class="content">
				<view class="price">{{ wallet.amount }}</view>
			</view>
		</view>
		<view class="center">
			<view class="smallSquare">
				<view class="box">
					<view class="num">{{ wallet.frozenTotal }}</view>
					<view class="text">提现冻结收入</view>
				</view>
			</view>
			<view class="smallSquare">
				<view class="box">
					<view class="num">{{ wallet.withdrawTotal }}</view>
					<view class="text">累计收入</view>
				</view>
			</view>
		</view>
		<view class="square">
			<view class="title">提现到微信零钱</view>
			<view class="name">提现金额</view>
			<view class="price">
				<view class="left">￥</view>
				<view class="right">
					<up-input v-model="query.amount" border="none" placeholder="最低提现金额100元"></up-input>
				</view>
			</view>
			<view class="name">真实姓名</view>
			<view class="price">
				<up-input v-model="query.realName" border="none" placeholder="超过2000元必须输入真实姓名"></up-input>
			</view>
			<view class="btn" @click="clickCashWithdrawal">立即提现</view>
			<view class="texts" @click="href(`/pages/common/richText?type=4`)">
				<up-icon name="info-circle-fill"></up-icon>
				<view class="text">提现说明</view>
			</view>
		</view>
		<view class="pageTitle">
			<view class="name">提现明细</view>
			<view class="time" @click="show = true">
				<view class="text">{{ query.queryYear + '-' + query.queryMonth }}</view>
				<up-icon name="arrow-down"></up-icon>
			</view>
		</view>
		<view class="" style="padding: 0 32rpx;">
			<view class="list" v-for="(item, index) in list" :key="index">
				<view class="item">
					<view class="name">提现-微信零钱</view>
					<view class="rig">+{{ item.amount }}</view>
				</view>
				<view class="item">
					<view class="time">{{ item.createTime.slice(0, 10) }}</view>
					<view class="btn" v-if="item.status == 1" @click="immediately(item)">{{ '立即领取' }}</view>
					<view class="btn2" v-else>{{ ['审核中', '', '已驳回', '已领取'][item.status] }}</view>
				</view>
			</view>
			<view class="emptyBlock" v-if="!list.length">
				<image src="/static/img32.png" mode="widthFix"></image>
			</view>
		</view>
		<up-datetime-picker :show="show" @cancel="show = false" @close="show = false" v-model="timeValue"
			:maxDate="Number(new Date()) + 1" @confirm="selectDate" mode="year-month"
			closeOnClickOverlay></up-datetime-picker>
	</view>
</template>

<script>
export default {
	data() {
		return {
			show: false,
			wallet: {},
			queryId: 0,
			timeValue: Number(new Date()) - 1,
			query: {
				pageNum: 1,
				pageSize: 10,
				queryYear: "",
				queryMonth: "",
				realName: "",
				amount: 0
			},
			list: [],
			mch_id: "",
			WxOpenAppId: "",
		}
	},
	onLoad(options) {
		this.queryId = options.queryId || 0
	},
	onShow() {
		let dateTime = this.timeYearMonth(Number(new Date()) - 1)
		this.query.queryYear = dateTime[0]
		this.query.queryMonth = dateTime[1]
		this.getMyWallet()
		this.getList()
	},
	methods: {
		async getMyWallet() {
			let res = await this.request('Money/MyWallet', {
				queryType: 0,
				queryId: this.queryId
			})
			this.wallet = res.data
		},
		async getList() {
			let res = await this.request("Money/WithdrawalList", this.query)
			this.list = res.data.items
			this.mch_id = res.data.mch_id
			this.WxOpenAppId = res.data.WxOpenAppId
		},
		// 日期
		selectDate(e) {
			console.log(e.value);
			let dateTime = this.timeYearMonth(e.value)
			this.query.queryYear = dateTime[0]
			this.query.queryMonth = dateTime[1]
			this.getList(true)
			this.show = false
		},
		immediately(item) {
			if (wx.canIUse('requestMerchantTransfer')) {
				wx.requestMerchantTransfer({
					mchId: this.mch_id,
					appId: this.WxOpenAppId,
					package: item.package,
					success: (res) => {
						if (res.err_msg == 'ok') {
							this.getMyWallet()
							this.getList()
						}
					},
					fail: (res) => {
						console.log('fail:', res);
					},
				});
			} else {
				wx.showModal({
					content: '你的微信版本过低，请更新至最新版本。',
					showCancel: false,
				});
			}
		},
		// 点击提现
		clickCashWithdrawal() {
			this.request('Money/Withdrawal', {
				...this.query,
				userType: this.queryId
			}, {}, false, true).then(res => {
				this.toast('提现申请成功')
				this.getMyWallet()
				this.getList()
			})
		},
	}
}
</script>

<style lang="scss" scoped>
.pageTitle {
	display: flex;
	justify-content: space-between;
	padding: 26rpx 32rpx;

	.name {
		font-weight: 600;
		font-size: 32rpx;
		color: #333333;
	}

	.time {
		display: flex;
		align-items: center;
		font-size: 36rpx;
		color: #000000;

		.text {
			margin-right: 16rpx;
		}
	}
}

.header {
	padding: 32rpx;

	.name {
		font-size: 30rpx;
		color: #2C1601;
	}

	.content {
		margin-top: 23rpx;
		display: flex;
		justify-content: space-between;
		align-items: center;

		.price {
			font-size: 48rpx;
			color: #2C1601;
		}
	}
}

.center {
	padding: 0 32rpx;
	display: flex;
	justify-content: space-between;

	.smallSquare {
		position: relative;
		background: white;
		width: 335rpx;
		height: 188rpx;
		box-shadow: 0rpx 3rpx 32rpx 1rpx rgba(109, 135, 196, 0.15);
		border-radius: 16 rpx;

		.box {
			padding: 32rpx;

			.num {
				margin-top: 40rpx;
				font-weight: bold;
				font-size: 40rpx;
				color: #2C1601;
			}

			.text {
				margin-top: 6rpx;
				font-size: 24rpx;
				color: #666666;
			}
		}
	}
}

.square {
	width: 686rpx;
	background: #FFFFFF;
	box-shadow: 0rpx 3rpx 32rpx 1rpx rgba(109, 135, 196, 0.16);
	border-radius: 16rpx;
	margin: 0 auto;
	padding: 32rpx;
	margin-top: 32rpx;

	.title {
		font-weight: 600;
		font-size: 32rpx;
		color: #000000;
	}

	.name {
		font-size: 30rpx;
		color: #333333;
		margin: 30rpx 0;
	}

	.price {
		display: flex;
		align-items: center;
		border-bottom: 1px solid #999;
		padding: 22rpx 0;

		.left {
			font-size: 35rpx;
			color: #333333;
			margin-right: 38rpx;
		}
	}

	.btn {
		margin-top: 32rpx;
		background: #8C4FFF;
		border-radius: 39rpx;
		display: flex;
		justify-content: center;
		align-items: center;
		height: 78rpx;
		font-size: 36rpx;
		color: #FFFFFF;
	}

	.texts {
		margin-top: 32rpx;
		color: #333;
		font-size: 32rpx;
		display: flex;
		align-items: center;

		.text {
			padding-left: 10rpx;
		}
	}

	.item {
		padding: 10rpx;
		font-size: 26rpx;
		color: #666666;
	}

}

.list {
	width: 100%;
	background-color: #FFFFFF;
	border-radius: 16rpx;
	padding: 32rpx;
	margin-bottom: 32rpx;

	.item {
		display: flex;
		justify-content: space-between;
		align-items: center;

		.name {
			font-size: 26rpx;
			color: #333333;
		}

		.rig {
			font-size: 26rpx;
			color: #D32C26;
		}

		.time {
			color: #666666;
			font-size: 22rpx;
		}

		.btn {
			background: #8C4FFF;
			color: white;
			font-size: 26rpx;
			padding: 10rpx 20rpx;
			border-radius: 36rpx;
		}

		.btn2 {
			background: #b9b9b9;
			color: white;
			font-size: 26rpx;
			padding: 10rpx 20rpx;
			border-radius: 36rpx;
		}
	}

	.item:not(:first-of-type) {
		margin-top: 12rpx;
	}
}
</style>